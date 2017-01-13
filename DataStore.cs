using System;
using System.Collections.Generic;
using BookRecommender.Models;

namespace BookRecommender.DataStore{
    public static class DataStore{
        public static IEnumerable<Book> GetBooks(){
            var list = new List<Book>(){
                new Book(){Name = "Book1", Id = 1},
                new Book(){Name = "Book2", Id = 2}
            };
            return list;
        }
    }
}