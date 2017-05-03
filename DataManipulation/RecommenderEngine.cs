using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BookRecommender.DataManipulation
{
    class RecommenderEngine
    {
        public List<int> RecommendBookSimilar(int bookId, int howMany = 6)
        {
            // MUCH OPTIMISATION AVAILABLE

            var timer = new Stopwatch();

            timer.Start();

            var db = new BookRecommenderContext();

            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }

            var authors = myBook.GetAuthors(db);
            var genres = myBook.GetGenres(db);
            var characters = myBook.GetCharacters(db);
            var publisher = myBook.Publisher;
            var lang = myBook.OrigLang;



            var listOfBooks = new List<int>();

            foreach (var author in authors)
            {
                listOfBooks.AddRange(author.GetBooksIds(db));
            }

            foreach (var genre in genres)
            {
                listOfBooks.AddRange(genre.GetBooksIds(db));
            }

            foreach (var character in characters)
            {
                listOfBooks.AddRange(character.GetBooksIds(db));
            }

            listOfBooks.AddRange(db.Books.Where(b => b.Publisher == publisher).Select(b => b.BookId));
            listOfBooks.AddRange(db.Books.Where(b => b.OrigLang == lang).Select(b => b.BookId));

            var groupedList = listOfBooks.GroupBy(b => b).
                     Select(group =>
                         new
                         {
                             BookId = group.Key,
                             Count = group.Count()
                         });
            var sortedList = groupedList.OrderByDescending(b => b.Count).Select(b => b.BookId);

            var recList = sortedList.Take(howMany + 1).ToList();

            // remove myself
            var wasRemoved = recList.Remove(bookId);

            if (!wasRemoved)
            {
                recList.RemoveAt(recList.Count - 1);
            }

            System.Console.WriteLine("--- ELAPSED TIME:" + timer.ElapsedMilliseconds);

            return recList;
        }
        public List<int> RecommendBookSimilarByTags(int bookId, int howMany = 6)
        {
            var db = new BookRecommenderContext();

            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }
            var tags = myBook.GetTags(db).Where(t => t.Score != null);

            
            var tagsLanguages = TopLang(tags.Select(t => t.Lang).Distinct());
            // var tagsLanguages = new string[] {"en"};

            // for each language find the most matching books by tags and then apply the merging function

            var listTopFromAllLangs = new List<(string lang, int bookId, double score)>();

            foreach (var lang in tagsLanguages)
            {
                var tagsInLang = tags.Where(t => t.Lang == lang);

                List<Tuple<int,double>> simTagsQuery = new List<Tuple<int, double>>();

                foreach (var tag in tagsInLang)
                {
                    var simTags = db.Tags.Where(t => (t.Value == tag.Value && t.Lang == lang && t.Score != null)).Select(t => new Tuple<int, double>(
                        t.BookId,
                        t.Score.Value * tag.Score.Value
                    ));
                    simTagsQuery.AddRange(simTags);
                }
                // generate top matches for lang
                var orderedFinalScore = simTagsQuery.GroupBy(t => t.Item1).Select(g => new
                {
                    BookId = g.Key,
                    FinalScore = g.Sum(s => s.Item2)
                }).OrderByDescending(t => t.FinalScore);

                // take top 10 from language and add it to final list
                var top = orderedFinalScore.Take(10);
                foreach (var score in top)
                {
                    listTopFromAllLangs.Add((lang, score.BookId, score.FinalScore));
                }
            }
            // Final aggregation between languages

            var finalList = listTopFromAllLangs.GroupBy(s => s.bookId).Select(
                g => new
                {
                    BookId = g.Key,
                    Score = g.Sum(s => PenalizeLang(s.score, s.lang))
                }
            ).OrderByDescending(t => t.Score).Select(t => t.BookId);

            var recList = finalList.Take(howMany + 1).ToList();

            // remove myself
            var wasRemoved = recList.Remove(bookId);
            if (!wasRemoved && recList.Count > 0)
            {
                recList.RemoveAt(recList.Count - 1);
            }
            return recList;
        }
        private double PenalizeLang(double score, string lang)
        {
            //TODO: IMPLEMENT LANGUAGE PENALIZATION
            return score;
        }
        private string[] TopLang(IEnumerable<string> langs){
            var db = new BookRecommenderContext();

            var langCounts = db.TagsCount.ToArray();

            return langs.Select(t => new {
                            Count = langCounts.Where(c => c.Lang == t).FirstOrDefault().Count,
                            Lang = t
                        })
                        .OrderByDescending(r => r.Count)
                        .Take(5)
                        .Select(q => q.Lang)
                        .ToArray();
        }
    }
}