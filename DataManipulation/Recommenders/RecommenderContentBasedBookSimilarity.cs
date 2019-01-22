using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderContentBasedBookSimilarity 
    {
        /// <summary>
        /// Recommender algorithm to search for books similar to one other.
        /// It is used on the Book page under the Similar books.
        /// We check for all books with similar attributes, for each matching attribute we add a point to the book.
        /// At the end, we return the books ordered by the most points.
        /// If user logged in, we remove already visited books from the recommendation.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <returns>Weighted List of books with the biggest similarity with the book from boookId</returns>
        public static List<Tuple<int, int>> RecommendWeightedList(int bookId, string userId = null)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            List<Tuple<int, int>> sortedListOfBookIDsAndTheirQuantities =
                LoadSimilarBookIDsAndTheirQuantitiesFromDatabase(bookId, db);

            // remove Already Rated Books
            return removeAlreadyRatedBooks(sortedListOfBookIDsAndTheirQuantities, userId, db);
        }

        /// <summary>
        /// Recommender algorithm to search for books similar to one other.
        /// It is used on the Book page under the Similar books.
        /// We check for all books with similar attributes, for each matching attribute we add a point to the book.
        /// At the end, we return the books ordered by the most points.
        /// If user logged in, we remove already visited books from the recommendation.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <returns>List of books with the biggest similarity with the book from boookId</returns>
        public static List<int> Recommend(int bookId, string userId = null)
        {
            List<Tuple<int, int>> recList = RecommendWeightedList(bookId, userId);

            return recList.Select(b => b.Item1).ToList();
        }


        /// <summary>
        /// Recommender algorithm to search for books similar to one other.
        /// It is used on the Book page under the Similar books.
        /// We check for all books with similar attributes, for each matching attribute we add a point to the book.
        /// At the end, we return the books ordered by the most points.
        /// If user logged in, we remove already visited books from the recommendation.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>Weighted List of books with the biggest similarity with the book from boookId</returns>
        public static List<Tuple<int, int>> RecommendWeightedList(int bookId, string userId = null, int howMany = 6)
        {
            List<Tuple<int, int>> recList = RecommendWeightedList(bookId, userId);

            return recList.Take(howMany).ToList();
        }

        /// <summary>
        /// Recommender algorithm to search for books similar to one other.
        /// It is used on the Book page under the Similar books.
        /// We check for all books with similar attributes, for each matching attribute we add a point to the book.
        /// At the end, we return the books ordered by the most points.
        /// If user logged in, we remove already visited books from the recommendation.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>List of books with the biggest similarity with the book from boookId</returns>
        public static List<int> Recommend(int bookId, string userId = null, int howMany = 6)
        {
            List<Tuple<int, int>> recList = RecommendWeightedList(bookId, userId, howMany);

            return recList.Select(b => b.Item1).ToList();
        }



       private static List<Tuple<int, int>> LoadSimilarBookIDsAndTheirQuantitiesFromDatabase(
           int bookId, BookRecommenderContext db)
        {
            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }

            // get data from the book we want to be the center of a search
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


            // group by the bookId and sum up the score,
            // then order the books to get the ones with the biggest sum on the begining
            List<Tuple<int, int>> listOfBookIdAndTheirQuantities = listOfBooks.GroupBy(b => b)
                    .Select(group => new Tuple<int, int>(group.Key, group.Count())).ToList();

            List<Tuple<int, int>> sortedListOfBookIDsAndTheirQuantities =
                    listOfBookIdAndTheirQuantities.OrderByDescending(b => b.Item2).ToList();

            // removes input bookId;
            List<Tuple<int, int>> result = sortedListOfBookIDsAndTheirQuantities
                    .Where(b => b.Item1 != bookId).ToList();

            return result;
        }

       private static List<Tuple<int, int>> removeAlreadyRatedBooks(
           List<Tuple<int, int>> listOfBookIDsAndTheirQuantities,
           string userId, BookRecommenderContext db)
        {
            // if user signed in, remove already rated books
            if (userId == null)
            {
                return listOfBookIDsAndTheirQuantities;
            }

            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return listOfBookIDsAndTheirQuantities;
            }

            // get all ratings
            List<BookRating> userRatings = db.Ratings
                .Where(r => r.UserId == userId).ToList();

            // remove books which user already rated
            return listOfBookIDsAndTheirQuantities.Where(
                    b => !userRatings.Select(r => r.BookId).Contains(b.Item1)
                    ).ToList();
        }

    }
}