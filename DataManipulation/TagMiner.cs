using BookRecommender.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;
using BookRecommender.DataManipulation.Stemmers;

namespace BookRecommender.DataManipulation
{
    class TagMiner
    {
        IStemmer EnglishStemmer;
        public static void Mine(List<int> methodNumber)
        {
            if (methodNumber.Count == 0)
            {
                MineBookTags();
            }
            if (methodNumber.Contains(0))
            {
                MineBookTags();
            }
        }
        public static void MineBookTags()
        {
            var dir = "C:\\netcore\\booksWikiPages\\";
            var db = new BookRecommenderContext();
            var booksWithWikiPage = db.Books.Where(b => b.WikipediaPage != null)
                                            .Select(b => ValueTuple.Create<int, string>(b.BookId, b.WikipediaPage));
            System.Console.WriteLine("Mining from wiki.");
            RetrieveAndParseWikiPages(booksWithWikiPage, dir);

            System.Console.WriteLine("Stemming documents...");
            var stemmedDocumentsDictionary = StemDocuments(dir);
            System.Console.WriteLine("Counting Idf...");
            var idfDic = GetIdf(stemmedDocumentsDictionary);
            System.Console.WriteLine("computing TdIdf...");
            var tdIdf = ComputeTdIdf(stemmedDocumentsDictionary, idfDic);
            System.Console.WriteLine("Saving tags to database");
            SaveBookTagsToDb(tdIdf);
        }
        static void RetrieveAndParseWikiPages(IEnumerable<(int id, string wikiPageUrl)> dataList, string dirWhereToSave)
        {
            if (!dirWhereToSave.EndsWith("\\"))
            {
                dirWhereToSave += "\\";
            }
            var wikiMiner = new WikipediaMiner();
            var total = dataList.Count();
            System.Console.WriteLine("Running:");
            
            using (var counter = new Counter(total))
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };

                Directory.CreateDirectory(dirWhereToSave);

                var parLoop = Parallel.ForEach<(int id, string wikiPageUrl)>(dataList, options, wikiEntry =>
                {
                    var wikiUrl = wikiEntry.wikiPageUrl;

                    var path = dirWhereToSave + wikiEntry.id;
                    try
                    {
                        if (!File.Exists(path))
                        {
                            var data = wikiMiner.MineAndParse(wikiUrl).Result;
                            if (data == null)
                            {
                                System.Console.WriteLine("Timeout: " + wikiUrl);
                            }
                            else
                            {
                                File.AppendAllText(path, data);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }
                    counter.Update();
                });

                System.Console.WriteLine("Done");
            }
        }

        static void SaveBookTagsToDb(Dictionary<string, List<(string word, double score)>> Tags)
        {
            var db = new BookRecommenderContext();
            using (var counter = new Counter(Tags.Count))
            {
                foreach (var document in Tags)
                {
                    var bookIdString = document.Key;

                    int bookId;
                    var ok = int.TryParse(bookIdString, out bookId);
                    if (!ok)
                    {
                        throw new InvalidDataException("BookId invalid: \"" + bookIdString + "\"");
                    }
                    var book = db.Books.Where(b => b.BookId == bookId)?.FirstOrDefault();

                    if (book == null)
                    {
                        if (book?.BookId != bookId)
                        {
                            throw new DataMisalignedException(bookId.ToString());
                        }
                        System.Console.WriteLine("Book with id not found: \"" + bookIdString + "\"");
                        continue;
                    }
                    foreach (var tag in document.Value)
                    {
                        var newTag = new Tag()
                        {
                            Value = tag.word,
                            Score = tag.score
                        };
                        book.AddTag(newTag, db);
                    }
                    counter.Update();
                }
            }
            db.SaveChanges();
        }
        static Dictionary<string, List<string>> StemDocuments(string dictionaryPath)
        {
            var files = Directory.GetFiles(dictionaryPath);
            var stemmedDocumentsDictionary = new Dictionary<string, List<string>>();
            IStemmer stemmer = new EnglishStemmer();
            object dictionaryLock = new Object();

            using (var counter = new Counter(files.Length))
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
                Parallel.ForEach<string>(files, options, file =>
                {
                    var fileName = Path.GetFileName(file);
                    var text = File.ReadAllText(file);
                    var stemmedWords = text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(w => stemmer.StemWord(w)).ToList();
                    lock (dictionaryLock)
                    {
                        stemmedDocumentsDictionary.Add(fileName, stemmedWords);
                    }
                    counter.Update();
                });
                // foreach (var file in files)
                // {
                //     var fileName = Path.GetFileName(file);
                //     var text = File.ReadAllText(file);
                //     var stemmedWords = text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                //                            .Select(w => Stemmer.StemEnglishWord(w)).ToList();
                //     stemmedDocumentsDictionary.Add(fileName, stemmedWords);
                //     counter.Update();
                // }
            }
            return stemmedDocumentsDictionary;
        }
        static Dictionary<string, List<(string word, double score)>> ComputeTdIdf(Dictionary<string, List<string>> stemmedDocuments, Dictionary<string, double> idfDic)
        {

            var retDictionary = new Dictionary<string, List<(string word, double score)>>();
            using (var counter = new Counter(stemmedDocuments.Count))
            {
                foreach (var document in stemmedDocuments)
                {
                    var docLenght = document.Value.Count;

                    var wordsWithCount = from w in document.Value
                                         group w by w into g
                                         select new { word = g.Key, count = g.Count() };


                    var tdidf = wordsWithCount.Select(w =>
                                                  new ValueTuple<string, double>(
                                                      w.word,
                                                      ((double)w.count / docLenght) * idfDic[w.word]))
                                              .OrderByDescending(w => w.Item2);

                    var tdidfTop10 = tdidf.Take(10).ToList();
                    retDictionary.Add(document.Key, tdidfTop10);
                    counter.Update();
                }
            }
            return retDictionary;
        }
        static Dictionary<string, double> GetIdf(Dictionary<string, List<string>> stemmedDocuments)
        {
            var wordCountDic = new Dictionary<string, int>();

            using (var counter = new Counter(stemmedDocuments.Count))
            {
                // Count in how many documents the word appears
                foreach (var document in stemmedDocuments)
                {
                    var stemmedUniqueWords = document.Value.Distinct();
                    foreach (var word in stemmedUniqueWords)
                    {
                        int value;
                        wordCountDic[word] = wordCountDic.TryGetValue(word, out value) ? ++value : 1;
                    }
                    counter.Update();
                }
            }
            var retDictionary = new Dictionary<string, double>();
            // count idf for each word as log(#doc / #doc where appeared)
            foreach (var word in wordCountDic)
            {
                var docWithWordCount = word.Value;
                var idf = Math.Log10((double)stemmedDocuments.Count / docWithWordCount);
                retDictionary[word.Key] = idf;
            }
            return retDictionary;
        }
        IStemmer GetStemmer(string lang)
        {
            if (EnglishStemmer == null)
            {
                EnglishStemmer = new EnglishStemmer();
            }
            return EnglishStemmer;
        }
    }
}