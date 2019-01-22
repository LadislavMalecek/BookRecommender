using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderMostPopular
    {
        /// <summary>
        /// Simple recommendation method which takes the latest 1000 ratings, then group the ratings
        /// by the bookId and sums all awarded scores, then we will order the books by the sum of scores
        /// and returns those with the biggest scores.
        /// If the user is logged in, we remove books already rated by him from the recommendation.
        /// </summary>
        /// <param name="howMany">How many books to return</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <returns>List of most popular latest books</returns>
        public static List<int> Recommend(int howMany, string userId = null)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var mostPopularBooks = db.Ratings
                             .OrderByDescending(r => r.CreatedTime)
                             .Take(1000)
                             .Select(r => new {Id = r.BookId, Rating = r.Rating})
                             .ToList();


            var groupedList = mostPopularBooks.GroupBy(r => r.Id).
                     Select(group =>
                         new
                         {
                             BookId = group.Key,
                             Score = group.Sum(i => i.Rating)
                         });
            var sortedList = groupedList.OrderByDescending(b => b.Score).Select(b => b.BookId);

            if (userId != null)
            {
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    // get all ratings
                var userRatings = db.Ratings.Where(r => r.UserId == userId).ToList();

                // remove books which user already rated
                sortedList = sortedList.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b));
                }
            }

            return sortedList.Take(howMany).ToList();
        }

    }
}