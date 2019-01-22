using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;


namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderTagBasedBookSimilarity
    {

        /// <summary>
        /// This method recommend books by the similarity in tags. We have a single book with which
        /// we make as a center of our similarity search. Then we will query all tags with similar value and
        /// language. We take only tags from the top 5 language list. Then we will count similarity score 
        /// between all pairs. Then we will group these books by their id and sum the scores. (If some books have similarity in
        /// multiple languages, we will sum the separate scores). Then we chose the books with highest scores,
        /// removing books user has already rated. And return the final list. 
        /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>List of recommended books</returns>
        public static List<int> Recommend(int bookId, string userId = null, int howMany = 6)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }
            var tags = myBook.GetTags(db).Where(t => t.Score != null);


            var tagsLanguages = TopLang(tags.Select(t => t.Lang).Distinct());

            // for each language find the most matching books by tags and then apply the merging function
            var listTopFromAllLangs = new List<(string lang, int bookId, double score)>();

            foreach (var lang in tagsLanguages)
            {
                var tagsInLang = tags.Where(t => t.Lang == lang);

                var simTagsQuery = new List<ItemWeight<int>>();

                foreach (var tag in tagsInLang)
                {
                    var simTags = db.Tags.Where(t => (t.Value == tag.Value && t.Lang == lang && t.Score != null)).Select(t =>
                    new ItemWeight<int>(
                        t.BookId,
                        t.Score.Value * tag.Score.Value));
                    simTagsQuery.AddRange(simTags);
                }
                // generate top matches for lang
                var orderedFinalScore = simTagsQuery.GroupBy(t => t.itemId).Select(g => new
                {
                    BookId = g.Key,
                    FinalScore = g.Sum(s => s.itemWeight)
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

            // if user signed in, remove already rated books
            if (userId != null)
            {
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    // get all ratings
                var userRatings = db.Ratings.Where(r => r.UserId == userId).ToList();

                // remove books which user already rated
                finalList = finalList.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b)).ToList();
                }
            }

            var recList = finalList.Take(howMany + 1).ToList();

            // remove myself
            var wasRemoved = recList.Remove(bookId);
            if (!wasRemoved && recList.Count > 0)
            {
                recList.RemoveAt(recList.Count - 1);
            }
            return recList;
        }
        private static double PenalizeLang(double score, string lang)
        {
            //TODO: IMPLEMENT LANGUAGE PENALIZATION
            return score;
        }
        private static string[] TopLang(IEnumerable<string> langs)
        {
            var db = new BookRecommenderContext();

            var langCounts = db.TagsCount.ToArray();

            return langs.Select(t => new
            {
                Count = langCounts.Where(c => c.Lang == t).FirstOrDefault().Count,
                Lang = t
            })
                        .OrderByDescending(r => r.Count)
                        .Take(5)
                        .Select(q => q.Lang)
                        .ToArray();
        }

        // due to a bug in value tuple, created as a hotfix for method below
        struct HelpStruct
        {
            public int bookId;
            public string userId;
            public int rating;
        }

        struct ItemWeight<T>
        {
            public ItemWeight(T itemId, double itemWeight)
            {
                this.itemId = itemId;
                this.itemWeight = itemWeight;
            }
            public T itemId;
            public double itemWeight;
        }

    }
}