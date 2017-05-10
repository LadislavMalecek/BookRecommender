using BookRecommender.DataManipulation;
using System.Collections.Generic;
using BookRecommender.Models.Database;

namespace BookRecommender.Models.AuthorViewModels
{
    public class AuthorDetail
    {
        public Author Author { get; set; }
        public IEnumerable<Book> Books { get; set; }
    }
}