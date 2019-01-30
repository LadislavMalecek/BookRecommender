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
        public static List<int> RecommendMostPopular(int howMany, string userId = null)
        {
            return RecommenderMostPopular.Recommend(howMany, userId);
        }

        public static List<int> RecommendForUserUBased(string userId, int howMany = 6)
        {
            return RecommenderPersonalizedContentBased.Recommend(userId, howMany);
        }

        public static List<int> RecommendForUserCBased(string userId, int howMany = 6)
        {
            return RecommenderUserBasedKNN.Recommend(userId, howMany);
        }

        public static List<int> RecommendForDiversityEnhancedBookSimilarity(int bookId, string userId = null, int howMany = 6)
        {
            var timer = new Stopwatch();
            timer.Start();

            List<int> result = RecommenderDiversityEnhancedBookSimilarity.Recommend(bookId, userId, 0.2, howMany);
            //List<int> result = RecommenderDummy.Recommend(bookId, userId, howMany);

            System.Console.WriteLine($"DiversityEnhancedBookSimilarity recommendation took: {timer.ElapsedMilliseconds}ms");
            return result;
        }
        
        public static List<int> RecommendBookSimilarByTags(int bookId, string userId = null, int howMany = 6)
        {
            return RecommenderTagBasedBookSimilarity.Recommend(bookId, userId, howMany);
        }

        public static List<int> RecommendSpreadingActivation(int bookId,
            SimilarityCacheModels simCacheModel, string userId = null, int howMany = 6)
        {
            var sw = Stopwatch.StartNew();

            RecommenderSpreadingActivation recommender = new RecommenderSpreadingActivation(2, 500, 50, simCacheModel);
            List<int> result = recommender.Recommend(bookId, userId, howMany);

            System.Console.WriteLine($"Spreading recommendation took: {sw.ElapsedMilliseconds}ms");
                        
            return result;
        }        
    }
}