using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderDummy
    {
        private static int BOOK_COUNT = 8;

        /// <summary>
        /// Dummy Recommender algorithm. Returns 8 (BOOK_COUNT) inputs bookIds.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>List of books full of bookID from argument</returns>
        public static List<int> Recommend(int bookId, string userId = null, int howMany = 6)
        {
            var listOfBooks = new List<int>();

            for (int i = 0; i < BOOK_COUNT; i++)
            {
                listOfBooks.Add(bookId);
            }
            return listOfBooks;
        }
    }
}