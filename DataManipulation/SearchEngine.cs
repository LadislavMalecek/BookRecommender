

using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation
{
    /// <summary>
    /// Search engine drives the search of entities in the database.
    /// </summary>
    class SearchEngine
    {
        /// <summary>
        /// Simpler and faster version of search to be used in dynamically loaded whispering when searching.
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="query">Current query</param>
        /// <param name="howManyTop">how many to show</param>
        /// <returns>List of recommendations</returns>
        public static List<string> Autocomplete(BookRecommenderContext db, string query, int howManyTop){
            if(string.IsNullOrEmpty(query)){
                return new List<string>();
            }
            var booksStartingWith = db.Books.Select(b => b.NameEn).Where(b => b.ToLower().StartsWith(query.ToLower())).Take(howManyTop).ToList();
            var authorsStartingWith = db.Authors.Select(a => a.NameEn).Where(a => a.ToLower().StartsWith(query.ToLower())).Take(howManyTop).ToList();
            var startsWith = booksStartingWith.Concat(authorsStartingWith);
            
            var booksContaining = db.Books.Select(b => b.NameEn).Where(b => b.ToLower().Contains(query.ToLower())).Take(howManyTop).ToList();
            var authorsContaining = db.Authors.Select(a => a.NameEn).Where(a => a.ToLower().Contains(query.ToLower())).Take(howManyTop).ToList();
            var contains = booksContaining.Concat(authorsContaining);

            var final = startsWith.Concat(contains).Distinct().Take(howManyTop);

            return final.ToList();
        }

        /// <summary>
        /// Main algorithm to return book searches from the database. We start with prefering the whole string matches
        /// and then starts to include more and more granular results, by words count match
        /// /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="query">Current query</param>
        /// <returns>List of returned books</returns>
        public static IEnumerable<Book> SearchBook(BookRecommenderContext db, string query)
        {
            // First try to find the exact query within books
            var startsWithFound = db.Books.Where(b => b.NameEn.ToLower().StartsWith(query.ToLower()));
            var containsFound = db.Books.Where(b => b.NameEn.ToLower().Contains(query.ToLower()));
            var wholeQueryFound = startsWithFound.Concat(containsFound);


            var splitQuery = query.ToLower().Split(new char[] { ' ' });

            // if query is only one word - return
            if(splitQuery.Length == 1){
                return wholeQueryFound;
            }

            // then find books which have even only single query word, and order them by the amounts of query words found
            var countDictionary = new Dictionary<Book, int>();
            foreach (var queryWord in splitQuery)
            {
                var books = db.Books.Where(b => b.NameEn.ToLower().Contains(queryWord));
                foreach (var book in books)
                {
                    int value;
                    countDictionary[book] = countDictionary.TryGetValue(book, out value) ? ++value : 1;
                }
            }
            var orderedResult = countDictionary.OrderByDescending(b => b.Value).Select(b => b.Key);


            //combine the whole query result with single words query result in way that whole query is preffered
            var combQuery = wholeQueryFound.Concat(orderedResult).Distinct();
            return combQuery;
        }
        /// <summary>
        /// Main algorithm to return author searches from the database. We start with prefering the whole string matches
        /// and then starts to include more and more granular results, by words count match
        /// /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="query">Current query</param>
        /// <returns>List of returned books</returns>
        public static IEnumerable<Author> SearchAuthor(BookRecommenderContext db, string query)
        {
            // First try to find the exact query within books
            var startsWithFound = db.Authors.Where(a => a.NameEn.ToLower().StartsWith(query.ToLower()));
            var containsFound = db.Authors.Where(a => a.NameEn.ToLower().Contains(query.ToLower()));
            var wholeQueryFound = startsWithFound.Concat(containsFound);

            var splitQuery = query.ToLower().Split(new char[] { ' ' });

            // if query is only one word - return
            if(splitQuery.Length == 1){
                return wholeQueryFound;
            }

            // then find books which have even only single query word, and order them by the amounts of query words found
            var countDictionary = new Dictionary<Author, int>();
            foreach (var queryWord in splitQuery)
            {
                var authors = db.Authors.Where(a => a.NameEn.ToLower().Contains(queryWord));
                foreach (var author in authors)
                {
                    int value;
                    countDictionary[author] = countDictionary.TryGetValue(author, out value) ? ++value : 1;
                }
            }
            var orderedResult = countDictionary.OrderByDescending(b => b.Value).Select(b => b.Key);


            //combine the whole query result with single words query result in way that whole query is preffered
            var combQuery = wholeQueryFound.Concat(orderedResult).Distinct();
            return combQuery;
        }
    }
}