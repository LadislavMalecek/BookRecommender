using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation.Recommender;


namespace BookRecommender.DataManipulation.Recommender.Test
{
    public class Test
    {
        private static int HOW_MANY_REC = 27;

        private static List<int> ReadDataset(String file)
        {
            string[] lines = System.IO.File.ReadAllLines(@file);
            List<String> linesList = new List<String>(lines);
            linesList = linesList.Skip(1).ToList();

            List<int> bookIds = linesList.Select(b => Int32.Parse(
                b.Substring(0, b.IndexOf(",")))).ToList();

            return bookIds;
        }

        /// <summary>
        /// Spreading activation recommender.
        /// </summary>
        public static void RunTest()
        {
            System.Console.WriteLine($"Testing:");

            List<int> bookIds = ReadDataset("testSetBookIDs.csv");

            TestRecContentBasedBookSimilarity(bookIds);

            TestRecTagBasedBookSimilarity(bookIds);

            TestRecDiversityEnhancedBookSimilarity(bookIds, 0.4);

            TestRecSpreadingActivation(bookIds, 5, 500, 500);
        }

      private static void TestRecContentBasedBookSimilarity(List<int> bookIds)
      {
            List<Recommendation> recommendationsRCBBS = new List<Recommendation>();
            String comment = "userId=null";
            foreach (int bookIdI in bookIds)
            {
                List<int> recommendationListI = RecommenderContentBasedBookSimilarity.Recommend(bookIdI, null, HOW_MANY_REC);

                Recommendation recommendationI = new Recommendation(bookIdI, recommendationListI);
                
                recommendationsRCBBS.Add(recommendationI);
                System.Console.WriteLine(recommendationI.exportAsString());
            }
            String fileName = "RecommenderContentBasedBookSimilarity.rec";
            ExportsRecommendationsToFile(recommendationsRCBBS, comment, fileName);
      }

        private static void TestRecTagBasedBookSimilarity(List<int> bookIds)
        {
            List<Recommendation> recommendationsRTBBS = new List<Recommendation>();
            String comment = "userId=null";
            foreach (int bookIdI in bookIds)
            {
                List<int> recommendationListI = RecommenderTagBasedBookSimilarity.Recommend(bookIdI, null, HOW_MANY_REC);

                Recommendation recommendationI = new Recommendation(bookIdI, recommendationListI);
                
                recommendationsRTBBS.Add(recommendationI);
                System.Console.WriteLine(recommendationI.exportAsString());
            }

            String fileName = "RecommenderTagBasedBookSimilarity.rec";
            ExportsRecommendationsToFile(recommendationsRTBBS, comment, fileName);

        }

        private static void TestRecDiversityEnhancedBookSimilarity(List<int> bookIds, double lambda)
        {
            List<Recommendation> recommendationsRDEBS = new List<Recommendation>();
            String commentRDEBS = "userId=null, lambda=" + lambda;
            foreach (int bookIdI in bookIds)
            {
                List<int> recommendationListI = RecommenderDiversityEnhancedBookSimilarity.Recommend(bookIdI, null,  lambda, HOW_MANY_REC);

                Recommendation recommendationI = new Recommendation(bookIdI, recommendationListI);
                
                recommendationsRDEBS.Add(recommendationI);
                System.Console.WriteLine(recommendationI.exportAsString());
            }
            String fileName = "RecommenderDiversityEnhancedBookSimilarityLambda0" +(int)lambda*10 + ".rec";
            ExportsRecommendationsToFile(recommendationsRDEBS, commentRDEBS, fileName);
        }

        private static void TestRecSpreadingActivation(List<int> bookIds, int maxLevel,
            int numberOfNSimilarNeighbors, int numberOfNeighborsForNextLevel)
        {
            List<Recommendation> recommendationsRSA = new List<Recommendation>();
            String commentRDEBS = "userId=null, maxLevel=" + maxLevel + ", numberOfSimilarNeighbors=" + numberOfNSimilarNeighbors + ", numberOfNeighborsForNextLevel=" + numberOfNeighborsForNextLevel + ", backSpreading=yes";
            BookRecommenderContext db = new BookRecommenderContext();
            SpreadingRecommenderCache model = new SpreadingRecommenderCache();
            model.Initialize(db);
            foreach (int bookIdI in bookIds)
            {
                RecommenderSpreadingActivation recommender = new RecommenderSpreadingActivation(
                    maxLevel, numberOfNSimilarNeighbors, numberOfNeighborsForNextLevel, new SimilarityCacheModels(model));
                List<int> recommendationListI = recommender.Recommend(bookIdI, null, HOW_MANY_REC);

                Recommendation recommendationI = new Recommendation(bookIdI, recommendationListI);
                
                recommendationsRSA.Add(recommendationI);
                System.Console.WriteLine(recommendationI.exportAsString());
            }
            String fileName = "RecommenderSpreadingActivationLevel" + maxLevel + "SimilarNeighbors" + numberOfNSimilarNeighbors + "NeighborsForNextLevel" + numberOfNeighborsForNextLevel + ".rec";
            ExportsRecommendationsToFile(recommendationsRSA, commentRDEBS, fileName);
        }

        private static void ExportsRecommendationsToFile(
            List<Recommendation> recommendations, String comment, String file)
        {
            String contents = "# " + comment + "\n";
            foreach (Recommendation recI in recommendations)
            {
                contents += recI.exportAsString() + "\n";
            }

            System.IO.File.WriteAllText(@file, contents);
        }
    }

    class Recommendation
    {
        public int bookId;
        public List<int> recommendation;

        public Recommendation(int boodId, List<int> recommendation)
        {
            this.bookId = boodId;
            this.recommendation = recommendation;
        }

        public string exportAsString()
        {
            return bookId + " : [" + String.Join(", ", recommendation.ToArray()) + "]";
        }
    }
}