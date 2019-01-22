using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderPersonalizedContentBased 
    {

        /// <summary>
        /// We will find the most similar users and then return books they have liked the most.
        /// </summary>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>List of recommended books</returns>
        public static List<int> Recommend(string userId, int howMany = 6)
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

            // Take books from the similar users, from the Top user and top rating down.
            foreach (var score in new int[] { 5, 4, 3 })
            {
                foreach (var closestUser in closestFourUsers)
                {
                    var potentialBooks = db.Ratings.Where(r => r.UserId == closestUser.UserId && r.Rating == score).ToList();
                    var pBNotYetSeen = potentialBooks.Where(b => !userRatings.Select(r => r.Book).Contains(b.BookId)).ToList();
                    recommendedBooks.AddRange(pBNotYetSeen.Select(b => b.BookId));
                    if (recommendedBooks.Distinct().Count() >= howMany)
                    {
                        return recommendedBooks.Distinct().ToList();
                    }
                }
            }
            return recommendedBooks.Distinct().ToList();
        }

    }

    // due to a bug in value tuple, created as a hotfix for method below
    struct HelpStruct
    {
        public int bookId;
        public string userId;
        public int rating;
    }
    
}
