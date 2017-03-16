

using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.Models;

namespace BookRecommender.DataManipulation
{
    class SearchEngine
    {
        public static IEnumerable<Book> SearchBook(BookRecommenderContext db, string query)
        {
            // First try to find the exact query within books
            var wholeQueryFound = db.Books.Where(b => b.NameEn.ToLower().Contains(query.ToLower()));

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

        public static IEnumerable<Author> SearchAuthor(BookRecommenderContext db, string query)
        {
            // First try to find the exact query within books
            var wholeQueryFound = db.Authors.Where(a => a.NameEn.ToLower().Contains(query.ToLower()));

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