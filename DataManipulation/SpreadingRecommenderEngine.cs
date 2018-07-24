using System.Collections.Generic;
using System.Linq;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.DataManipulation
{
    public class SpreadingRecommenderEngine
    {
        public List<Book> RecommendBooksSimilarBySpreadingActivation(List<int> startingBooksIds, int howMany = 6, int howManySpreads = 50)
        {
            var db = new BookRecommenderContext();
            // dequeue only - used to take all books from one rating level
            var evalQueue = new Queue<Book>();
            // enqueue only - used to store books which will be used in eval queue in the next level
            var nextEvalQueue = new Queue<Book>();

            double currentRatingLevel = 1d;
            int currentSpreadCount = 0;
            bool isSpreadOver = false;

            // book and its current rating
            var ratingForObjectsDictionary = new Dictionary<Book, double>();

            // Not using .Include bsc of simplicity with previous work, would have to rewrite huge part of the app
            var startingBooks = db.Books
                .Where(b => startingBooksIds.Contains(b.BookId))
                .ToList();

            EnqueueMany(evalQueue, startingBooks);

            while (currentSpreadCount < howManySpreads && !isSpreadOver)
            {
                foreach (var itemToEval in evalQueue.AsEnumerable())
                {
                    RunOne(itemToEval, nextEvalQueue, ratingForObjectsDictionary, currentSpreadCount, db);
                    currentSpreadCount++;
                    if (currentSpreadCount >= howManySpreads)
                    {
                        break;
                    }
                }
                evalQueue = nextEvalQueue;
                nextEvalQueue = new Queue<Book>();
                currentRatingLevel /= 2;
                if(evalQueue.Count == 0)
                {
                    isSpreadOver = true;
                }
            }

            return ratingForObjectsDictionary
                .OrderByDescending(x => x.Value)
                .Take(howMany)
                .Select(x => x.Key)
                .ToList();
        }

        private void RunOne(Book nextToEval, Queue<Book> evalQueue, Dictionary<Book, double> ratingDic, double currentLevelRating, BookRecommenderContext db)
        {
            var booksAuthors = nextToEval.GetAuthors(db);
            var booksGenres = nextToEval.GetGenres(db);
            // var booksCharacters = nextToEval.GetCharacters(db);
            // var booksTags = nextToEval.GetTags(db);

            foreach (var author in booksAuthors)
            {
                var books = author.GetBooks(db).ToList();
                IncreaseCurrentForMany(books, ratingDic, currentLevelRating);
                EnqueueMany(evalQueue, books);
            }
            foreach (var genre in booksGenres)
            {
                var books = genre.GetBooks(db).ToList();
                EnqueueMany(evalQueue, books);
                IncreaseCurrentForMany(books, ratingDic, currentLevelRating);
            }
        }

        private void IncreaseCurrent(Book nextToEval, Dictionary<Book, double> ratingDic, double increaseAmount)
        {
            ratingDic.TryGetValue(nextToEval, out var currentRatingOfNext);
            ratingDic[nextToEval] = currentRatingOfNext + increaseAmount;
        }
        private void IncreaseCurrentForMany(List<Book> booksToIncrease, Dictionary<Book, double> ratingDic, double increaseAmount)
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