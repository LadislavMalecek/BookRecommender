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
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

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
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

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

                var simTagsQuery = new List<(int bookId, double score)>();

                foreach (var tag in tagsInLang)
                {
                    var simTags = db.Tags.Where(t => (t.Value == tag.Value && t.Lang == lang && t.Score != null)).Select(t =>
                    new ValueTuple<int, double>(
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
        private string[] TopLang(IEnumerable<string> langs)
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

        public List<int> RecommendForUserUBased(string userId, int howMany = 6)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // get all positive ratings
            var userRatings = db.Ratings.Where(r => r.UserId == userId && r.Rating >= 3).Select(r => new
            {
                Rating = r.Rating,
                Book = r.BookId
            }).ToList();

            // load all ratings for same books
            //var simUserRatings = new List<(int bookId, string userId, int rating)>();
            // var simUserRatings = new List<(int bookId, string userId, int rating)>();
            var simUserRatings = new List<HelpStruct>();


            foreach (var rating in userRatings)
            {
                var simRatings = db.Ratings.Where(r => r.BookId == rating.Book && r.UserId != userId).ToList();
                var s = simRatings.Select(r =>
                new HelpStruct
                {
                    bookId = r.BookId,
                    userId = r.UserId,
                    rating = r.Rating
                });
                if (simRatings != null && simRatings.Any())
                {
                    simUserRatings.AddRange(s);
                }
            }

            // count book averages for users that has not rated the book
            var bookAvg = simUserRatings.GroupBy(r => r.bookId).Select(group =>
            new
            {
                BookId = group.Key,
                AvgScore = group.Sum(s => s.rating) / group.Count()
            }
            );



            //count similarities - use book avg if rating not available

            var simUsers = simUserRatings.GroupBy(r => r.userId).Select(group =>
            new
            {
                UserId = group.Key,
                // we want to evaluate every book for every user, so we will use bookAvg as a linq base element in score count
                // because it has all books
                Score = bookAvg.Sum(b =>
                {
                    int scoreA = userRatings.First(ra => ra.Book == b.BookId).Rating;

                    //if sim has data, use it, else use default from bookAvg
                    var hasData = group.Any(p => p.bookId == b.BookId);
                    var scoreB = hasData ? group.First(p => p.bookId == b.BookId).rating : b.AvgScore;
                    //count metrics
                    return Math.Pow(scoreA - scoreB, 2);
                })
            }
            );

            var closestFourUsers = simUsers.OrderBy(u => u.Score).Take(4).ToList();
            var recommendedBooks = new List<int>();


            foreach (var score in new int[] { 5, 4, 3 })
            {
                foreach (var closestUser in closestFourUsers)
                {
                    var potenionalBooks = db.Ratings.Where(r => r.UserId == closestUser.UserId && r.Rating == score).ToList();
                    var pBNotYetSeen = potenionalBooks.Where(b => !userRatings.Select(r => r.Book).Contains(b.BookId)).ToList();
                    recommendedBooks.AddRange(pBNotYetSeen.Select(b => b.BookId));
                    if (recommendedBooks.Distinct().Count() >= howMany)
                    {
                        return recommendedBooks.Distinct().ToList();
                    }
                }
            }
            return recommendedBooks;
        }

        public List<int> RecommendForUserCBased(string userId, int howMany = 6)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var booksToFindSimilarityFor = new List<int>();

            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // get all positive ratings
            var userRatings = db.Ratings.Where(r => r.UserId == userId && r.Rating >= 3).Select(r => new
            {
                Rating = r.Rating,
                Book = r.BookId,
                Date = r.CreatedTime
            }).OrderByDescending(o => o.Date).Take(6).Select(r => r.Book).ToList();

            booksToFindSimilarityFor.AddRange(userRatings);

            var userActionsBookViewed = db.UsersActivities.Where(a => a.UserId == userId &&
                                                            a.Type == Models.UserActivity.ActivityType.BookDetailViewed)
                                                .OrderByDescending(a => a.CreatedTime)
                                                .Take(6)
                                                .Select(a => int.Parse(a.Value)).ToList();

            booksToFindSimilarityFor.AddRange(userActionsBookViewed);



            var recommendedBooks = new List<int>();


            foreach (var book in booksToFindSimilarityFor)
            {
                var result = RecommendBookSimilar(book, 1);
                if(result.Count != 0){
                    recommendedBooks.Add(result[0]);
                    if(recommendedBooks.Distinct().Count() >= howMany){
                        return recommendedBooks.Distinct().ToList();
                    }
                }

            }
            return recommendedBooks.Distinct().ToList();
        }
    }
}