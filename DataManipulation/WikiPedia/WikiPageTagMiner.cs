using System;
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
    class WikiPageTagMiner
    {
        WikiPageDownloader downloader = new WikiPageDownloader();
        WikiPageStorage storage = new WikiPageStorage();
        WikiPageTrimmer trimmer = new WikiPageTrimmer();
        readonly bool verbose;
        public WikiPageTagMiner(bool verbose = true)
        {
            this.verbose = verbose;
        }

        public void UpdateTags(List<int> methodsList, MiningState miningState = null)
        {

            if (methodsList == null || methodsList.Count == 0)
            {
                throw new NotImplementedException();
            }
            if (methodsList.Contains(0))
            {
                // todo: dependency injection
                if (miningState != null)
                {
                    miningState.CurrentState = MiningStateType.RunningQueryingEndpoint;
                }
                var sparqlData = new WikiDataEndpointMiner().GetBooksWikiPages().ToList();
                DownloadAndTrimPages(sparqlData);

            }
            if (methodsList.Contains(1))
            {
                CalculateAndSaveBookTagsToDb(miningState);
            }
        }
        public void UpdateTags(int methodNumber, MiningState miningState = null){
            var list = new List<int>() { methodNumber };
            UpdateTags(list, miningState);
        }
        public void DownloadAndTrimPages(List<(string bookId, string wikiPageUrl)> wikiPages, MiningState miningState = null, int degreeOfParallelism = 7, bool skipAlreadyDownloaded = true)
        {
            if (miningState != null)
            {
                miningState.CurrentState = MiningStateType.Running;
            }
            var counterForMiningState = 0;
            // How many thread are going to be run in parallel
            var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
            // if disabled do not create counter
            using (var counter = verbose ? new Counter(wikiPages.Count, 5) : null)
            {

                var parLoop = Parallel.ForEach<(string bookId, string wikiPageUrl)>(wikiPages, options, page =>
                // foreach (var page in wikiPages)
                {
                    try
                    {
                        string lang = GetLangFromWikiUrl(page.wikiPageUrl);
                        bool skipDownload = skipAlreadyDownloaded && storage.PageExist(page.bookId, lang);
                        if (!skipDownload)
                        {
                            var downloadedPage = downloader.DownloadPage(page.wikiPageUrl).Result;
                            var trimmedPage = trimmer.Trim(downloadedPage);
                            storage.SavePage(trimmedPage, lang, page.bookId);
                        }
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText("C:\\netcore\\Log.txt", "\n--\nUrl: \"" + page.wikiPageUrl + "\", " + e.ToString());
                    }
                    if (miningState != null)
                    {
                        miningState.Message = string.Format("{1}/{2}",
                            counterForMiningState, wikiPages.Count);
                        counterForMiningState++;
                    }
                    if (verbose)
                    {
                        counter.Update();
                    }
                });
                // }
            }
            if (miningState != null)
            {
                miningState.CurrentState = MiningStateType.Completed;
                miningState.Message = DateTime.Now.ToString();
            }
        }
        string GetLangFromWikiUrl(string url)
        {
            // Example:
            // https://en.wikipedia.org/wiki/God_in_the_Age_of_Science%3F
            var splittedUrl = url?.Split(new char[] { '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return splittedUrl.Length >= 1 ? splittedUrl[1] : null;
        }

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
                // Update count table
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

                    var uri = endpoint.GetUriFromId(doc.docId);

                    var book = db.Books.Where(b => b.Uri == uri)?.FirstOrDefault();

                    if (book == null)
                    {
                        System.Console.WriteLine("Book with uri not found: \"" + uri + "\"");
                        continue;
                    }



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
                miningState.CurrentState = MiningStateType.Completed;
                miningState.Message = DateTime.Now.ToString();
            }
        }
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