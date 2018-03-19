using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BookRecommender.DataManipulation.Stemmers;
using BookRecommender.DataManipulation.WikiData;
using BookRecommender.Models;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.WikiPedia
{
    /// <summary>
    /// Wikipedia tag miner takes all tags from the wikipage storage and then calculates tags
    /// using TF-IDF metric. It does this separately for each language and
    /// then saves top 10 tags for each book wiki page to the database.
    /// </summary>
    class WikiPageTagMiner
    {
        WikiPageDownloader downloader = new WikiPageDownloader();
        WikiPageStorage storage = new WikiPageDatabaseStorage();
        WikiPageTrimmer trimmer = new WikiPageTrimmer();
        readonly bool verbose;
        MiningState miningState;
        int degreeOfParallelism = 7;
        bool skipAlreadyDownloaded = true;
        int counterForMiningState = 0;

        public WikiPageTagMiner(bool verbose = true)
        {
            this.verbose = verbose;
        }

        /// <summary>
        /// Generalized mining method, used from mining proxy and cmd miner
        /// </summary>
        /// <param name="methodsList">Which functions should execute</param>
        /// <param name="miningState">Link to mining proxy monitoring instance</param>
        public void UpdateTags(List<int> methodsList, MiningState miningState = null)
        {
            this.miningState = miningState;

            if (methodsList == null || methodsList.Count == 0)
            {
                throw new NotImplementedException();
            }
            if (methodsList.Contains(0))
            {
                DownloadAndTrimPages();
            }
            if (methodsList.Contains(1))
            {
                CalculateAndSaveBookTagsToDb();
            }
        }
        public void UpdateTags(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateTags(list, miningState);
        }

        void AddPagesToDownload(BlockingCollection<(string bookId, string lang, string text)> downloadList, IEnumerable<(string bookId, string wikiPageUrl)> pagesList, Counter counter, int counterForMiningState, int totalPages)
        {
            int internalCount = 0;
            foreach (var page in pagesList)
            {
                string lang = GetLangFromWikiUrl(page.wikiPageUrl);
                bool skipDownload = skipAlreadyDownloaded && storage.PageExist(page.bookId, lang);
                if (!skipDownload)
                {
                    downloadList.Add((page.bookId, lang, page.wikiPageUrl));
                }
                else
                {
                    if (verbose)
                    {
                        counter.Update();
                    }
                    if (miningState != null)
                    {
                        miningState.Message = string.Format("{0}/{1}",
                            counterForMiningState, totalPages);
                        counterForMiningState++;
                    }
                }
                if (internalCount % 1000 == 0)
                {
                    System.Console.WriteLine($"Added to download: {internalCount / 1000}k");
                }
                internalCount++;
            }
            downloadList.CompleteAdding();
        }

        void DownloadPages(BlockingCollection<(string bookId, string lang, string url)> downloadList, BlockingCollection<(string bookId, string lang, string text)> itemsDownloaded, Counter counter, int counterForMiningState, int totalPages)
        {
            int internalCount = 0;

            var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

            var parLoop = Parallel.ForEach<(string bookId, string lang, string url)>(downloadList.GetConsumingEnumerable(), options, page =>
            // foreach (var page in wikiPages)
            {
                try
                {
                    var downloadedPage = downloader.DownloadPage(page.url).Result;
                    var trimmedPage = trimmer.Trim(downloadedPage);
                    itemsDownloaded.Add((page.bookId, page.lang, trimmedPage));
                    if (verbose)
                    {
                        counter.Update();
                    }
                    if (miningState != null)
                    {
                        miningState.Message = string.Format("{0}/{1}",
                            counterForMiningState, totalPages);
                        counterForMiningState++;
                    }
                }
                catch (Exception e)
                {
                    // log exceptions
                    System.Console.WriteLine("\n--\nUrl: \"" + page.url + "\", " + e.ToString());
                    // File.AppendAllText("C:\\netcore\\Log.txt", "\n--\nUrl: \"" + page.wikiPageUrl + "\", " + e.ToString());
                }
                finally
                {
                    if (internalCount % 1000 == 0)
                    {
                        System.Console.WriteLine($"Downloaded: {internalCount / 1000}k");
                    }
                    internalCount++;
                }
            });
            itemsDownloaded.CompleteAdding();
        }

        void SaveDownloaded(BlockingCollection<(string bookId, string lang, string text)> itemsDownloaded, Counter counter, int pagesCount)
        {
            int internalCount = 0;

            foreach (var (bookId, lang, text) in itemsDownloaded.GetConsumingEnumerable())
            {
                storage.SavePage(text, lang, bookId);
                if (internalCount % 1000 == 0)
                {
                    System.Console.WriteLine($"Saved: {internalCount / 1000}k");
                }
                internalCount++;
            }
        }

        /// <summary>
        /// Main method for begining of mining action. It runs in parallel for a speeded up performance.
        /// Change degree of parallelism with caution, when limit too high, then Wikipedia will trigger DOS
        /// protection and blocks your connection.
        /// </summary>
        /// <param name="miningState">Link to mining proxy singleton monitoring instance</param>
        /// <param name="degreeOfParallelism">How many simultaneous operations should be executed</param>
        /// <param name="skipAlreadyDownloaded">True then page already in storage will be skiped.</param>
        public void DownloadAndTrimPages()
        {

            if (miningState != null)
            {
                miningState.CurrentState = MiningStateType.RunningQueryingEndpoint;
            }

            // Download wikipage address from endpoint
            // todo: Dependency injection
            var wikiPages = new WikiDataEndpointMiner().GetBooksWikiPages().ToList();

            BlockingCollection<(string bookId, string lang, string url)> downloadList = new BlockingCollection<(string bookId, string lang, string url)>();
            BlockingCollection<(string bookId, string lang, string text)> itemsDownloaded = new BlockingCollection<(string bookId, string lang, string text)>();

            var counter = new Counter(wikiPages.Count);

            if (miningState != null)
            {
                miningState.CurrentState = MiningStateType.Running;
            }
            counterForMiningState = 0;
            Task addAndSave = Task.Run(() =>
            {
                AddPagesToDownload(downloadList, wikiPages, counter, counterForMiningState, wikiPages.Count);
                SaveDownloaded(itemsDownloaded, counter, counterForMiningState);
            });

            Task download = Task.Run(() =>
            {
                DownloadPages(downloadList, itemsDownloaded, counter, counterForMiningState, wikiPages.Count);
            });

            Task.WaitAll(addAndSave, download);
        }
        ///
        string GetLangFromWikiUrl(string url)
        {
            // Example:
            // https://en.wikipedia.org/wiki/God_in_the_Age_of_Science%3F
            var splittedUrl = url?.Split(new char[] { '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return splittedUrl.Length >= 1 ? splittedUrl[1] : null;
        }

        /// <summary>
        /// Gets all tags from the local wikipedia storage and counts and then saves all tags using TF-IDF
        /// </summary>
        /// <param name="miningState">Mining state pointing to entity which we want to update</param>
        public void CalculateAndSaveBookTagsToDb(MiningState miningState = null)
        {
            var db = new BookRecommenderContext();
            var endpoint = new WikiDataEndpointMiner();
            var books = db.Books;
            var langs = storage.GetLangs();
            var langDoneCount = 0;
            if (miningState != null)
            {
                miningState.CurrentState = MiningStateType.Running;
            }
            foreach (var lang in langs)
            {
                var howManyLeft = langs.Count() - langDoneCount;
                System.Console.WriteLine("---");
                System.Console.WriteLine(lang);
                if (miningState != null)
                {
                    miningState.Message = string.Format("lang:{0}, left:{1} - {2}",
                            lang, howManyLeft, "Loading from disk");
                }
                System.Console.WriteLine("Loading pages from disk");
                var pages = storage.GetPagesInLang(lang);
                System.Console.Write("Loading pages from disk finished");

                var pagesCount = storage.PagesInLangCount(lang);

                // Update count table, used to speed up recommender engine
                TagCount tc = db.TagsCount.Where(t => t.Lang == lang).FirstOrDefault();
                if (tc == null)
                {
                    db.TagsCount.Add(new TagCount(lang, pagesCount));
                }
                else
                {
                    tc.Count = pagesCount;
                }
                db.SaveChanges();
                var documents = new List<(string docId, string[] words)>();
                if (miningState != null)
                {
                    miningState.Message = string.Format("lang:{0}, left:{1} - {2}",
                            lang, howManyLeft, "Parsing");
                }
                foreach (var page in pages)
                {
                    // basic parse
                    var words = page.text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


                    // stem if available
                    IStemmer stemmer = Stemmers.Stemmers.GetStemmerToLang(lang);
                    if (stemmer != null)
                    {
                        words = words.Select(w => stemmer.StemWord(w)).ToArray();
                    }
                    documents.Add((page.id, words));
                }
                if (miningState != null)
                {
                    miningState.Message = string.Format("lang:{0}, left:{1} - {2}",
                         lang, howManyLeft, "ComputingTdIdf");
                }
                var ratings = ComputeTdIdf(documents);
                // save ratings to database

                // remove all tags of current lang, we need this to avoid updating, that is very expensive
                db.Tags.RemoveRange(db.Tags.Where(t => t.Lang == lang));

                foreach (var doc in ratings)
                {
                    // check if book in database
                    var uri = endpoint.GetUriFromId(doc.docId);

                    var book = db.Books.Where(b => b.Uri == uri)?.FirstOrDefault();

                    if (book == null)
                    {
                        System.Console.WriteLine("Book with uri not found: \"" + uri + "\"");
                        continue;
                    }

                    // create tags
                    foreach (var tag in doc.ratings)
                    {
                        var newTag = new Tag()
                        {
                            Value = tag.word,
                            Score = tag.score,
                            Lang = lang
                        };
                        book.AddTag(newTag, db);
                    }
                }

                // write out info
                if (miningState != null)
                {
                    miningState.Message = string.Format("lang:{0}, left:{1} - {2}",
                            lang, howManyLeft, "Saving to database");
                }
                System.Console.WriteLine("Saving to database");
                db.SaveChanges();
                System.Console.Write("Saving to database finished");
                langDoneCount++;
                System.Console.WriteLine($"{lang} done, {howManyLeft} left");
            }
            if (miningState != null)
            {
                // update mining singleton
                miningState.CurrentState = MiningStateType.Completed;
                miningState.Message = DateTime.Now.ToString();
            }
        }
        /// <summary>
        /// Computes final td idf ratings. It first count in how many documents the word appears - IDF
        /// and then continues with with each document counting the TF and finally TF-IDF.
        /// </summary>
        /// <param name="parsedDocuments">Parsed and trimmed list of documnets</param>
        /// /// <returns>List of counted ratings for each document</returns>
        static List<(string docId, List<(string word, double score)> ratings)> ComputeTdIdf(List<(string docId, string[] words)> parsedDocuments, int howManyTop = 10)
        {
            // compute Idf:
            var wordCountDic = new Dictionary<string, int>();

            using (var counter = new Counter(parsedDocuments.Count))
            {
                // Count in how many documents the word appears
                foreach (var document in parsedDocuments)
                {
                    var stemmedUniqueWords = document.words.Distinct();
                    foreach (var word in stemmedUniqueWords)
                    {
                        int value;
                        wordCountDic[word] = wordCountDic.TryGetValue(word, out value) ? ++value : 1;
                    }
                    counter.Update();
                }
            }
            var idfDictionary = new Dictionary<string, double>();
            // count idf for each word as log(#doc / #doc where appeared)
            foreach (var word in wordCountDic)
            {
                var docWithWordCount = word.Value;
                var td = Math.Log10((double)parsedDocuments.Count / docWithWordCount);
                idfDictionary[word.Key] = td;
            }

            // compute TD-IDF
            var retList = new List<(string docId, List<(string word, double score)> ratings)>();
            using (var counter = new Counter(parsedDocuments.Count))
            {
                foreach (var document in parsedDocuments)
                {
                    // for each document, use term frequency * inverse document freq

                    var docLength = document.words.Length;

                    // count how many times words appear in document
                    var wordsWithCount = from w in document.words
                                         group w by w into g
                                         select new { word = g.Key, count = g.Count() };


                    var tdidf = wordsWithCount.Select(w =>
                                                  new ValueTuple<string, double>(
                                                      w.word,
                                                      ((double)w.count / docLength) * idfDictionary[w.word]))
                                              .OrderByDescending(w => w.Item2);

                    var tdidfTop = tdidf.Take(howManyTop).ToList();
                    retList.Add((document.docId, tdidfTop));
                    counter.Update();
                }
            }
            return retList;
        }
    }
}