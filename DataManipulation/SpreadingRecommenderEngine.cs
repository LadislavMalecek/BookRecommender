using System.Collections.Generic;
using System.Linq;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.DataManipulation
{
    public class SpreadingRecommenderEngine
    {
        private const int MAX_LEVEL = 1;

        private readonly BookRecommenderContext _db;
        private readonly SpreadingRecommenderCache _spreadingRecommenderCache;



        // dequeue only - used to take all books from one rating level
        Queue<int> evalQueue;
        // enqueue only - used to store books which will be used in eval queue in the next level
        Queue<int> nextEvalQueue;
        Dictionary<int, double> ratingForObjectsDictionary;


        double currentRatingLevel;
        int currentLevel;

        public SpreadingRecommenderEngine(
            BookRecommenderContext db,
            SpreadingRecommenderCache spreadingRecommenderCache)
        {
            _db = db;
            _spreadingRecommenderCache = spreadingRecommenderCache;
        }

        private void Initialize()
        {
            evalQueue = new Queue<int>();
            nextEvalQueue = new Queue<int>();
            currentRatingLevel = 1d;
            currentLevel = 0;
            ratingForObjectsDictionary = new Dictionary<int, double>();
        }

        public List<int> RecommendBooksSimilarBySpreadingActivation(List<int> startingBooksIds, int howMany = 6)
        {
            Initialize();
            // bookId and its current rating

            // Not using .Include bsc of simplicity with previous work, would have to rewrite huge part of the app
            var startingBooks = _db.Books
                .Where(b => startingBooksIds.Contains(b.BookId))
                .ToList();

            EnqueueMany(evalQueue, startingBooks.Select(b => b.BookId));

            while (currentLevel <= MAX_LEVEL)
            {
                System.Console.WriteLine($"EvalQueue size: {evalQueue.Count}");
                foreach (var itemToEval in evalQueue)
                {
                    RunOne(itemToEval, nextEvalQueue, ratingForObjectsDictionary, currentRatingLevel, _db);
                }


                System.Console.WriteLine($"Level {currentLevel} finished, next leven itemCount: {nextEvalQueue.Count}");
                System.Console.WriteLine("Top ten in rating:");

                var topTenBooks = ratingForObjectsDictionary.OrderByDescending(kvp => kvp.Value).Take(10);
                foreach(var topBook in topTenBooks)
                {
                    System.Console.WriteLine($"Id: {topBook.Key}, Rat: {topBook.Value}, Name: {_spreadingRecommenderCache.GetName(topBook.Key)}");
                }

                currentLevel++;
                evalQueue = nextEvalQueue;
                nextEvalQueue = new Queue<int>();
                currentRatingLevel /= 2;
            }

            return ratingForObjectsDictionary
                .OrderByDescending(x => x.Value)
                .Take(howMany)
                .Select(x => x.Key)
                .ToList();
        }

        private void RunOne(int nextToEval, Queue<int> evalQueue, Dictionary<int, double> ratingDic, double currentLevelRating, BookRecommenderContext db)
        {
            var simBooksByAll = _spreadingRecommenderCache.GetSimilarBooksByAll(nextToEval);

            IncreaseCurrentForMany(simBooksByAll, ratingDic, currentLevelRating);
            EnqueueMany(evalQueue, simBooksByAll);
        }

        private void IncreaseCurrent(int nextToEval, Dictionary<int, double> ratingDic, double increaseAmount)
        {
            ratingDic.TryGetValue(nextToEval, out var currentRatingOfNext);
            ratingDic[nextToEval] = currentRatingOfNext + increaseAmount;
        }
        private void IncreaseCurrentForMany(List<int> booksToIncrease, Dictionary<int, double> ratingDic, double increaseAmount)
        {
            foreach (var bookToIncrease in booksToIncrease)
            {
                ratingDic.TryGetValue(bookToIncrease, out var currentRatingOfNext);
                ratingDic[bookToIncrease] = currentRatingOfNext + increaseAmount;
            }
        }
        private void EnqueueMany<T>(Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}