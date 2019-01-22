using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation.Recommender;

namespace BookRecommender.DataManipulation
{
    class RecommenderEngine
    {
        public List<int> RecommendMostPopular(int howMany, string userId = null)
        {
            return RecommenderMostPopular.Recommend(howMany, userId);
        }

        public List<int> RecommendForUserUBased(string userId, int howMany = 6)
        {
            return RecommenderPersonalizedContentBased.Recommend(userId, howMany);
        }

        public List<int> RecommendForUserCBased(string userId, int howMany = 6)
        {
            return RecommenderUserBasedKNN.Recommend(userId, howMany);
        }

        public List<int> RecommendForDiversityEnhancedBookSimilarity(int bookId, string userId = null, int howMany = 6)
        {
            var timer = new Stopwatch();
            timer.Start();

            List<int> result = RecommenderDiversityEnhancedBookSimilarity.Recommend(bookId, userId, 0.2, howMany);
            //List<int> result = RecommenderDummy.Recommend(bookId, userId, howMany);

            System.Console.WriteLine("--- ELAPSED TIME:" + timer.ElapsedMilliseconds);

            return result;
        }
        
        public List<int> RecommendBookSimilarByTags(int bookId, string userId = null, int howMany = 6)
        {
            return RecommenderTagBasedBookSimilarity.Recommend(bookId, userId, howMany);
        }
        
    }
}