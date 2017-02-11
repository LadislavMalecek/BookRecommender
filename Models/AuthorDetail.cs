
using System.Collections.Generic;

namespace BookRecommender.Models{
    public class AuthorDetail{
        public Author Author { get; set; }
        public IEnumerable<Book> Books { get; set; }

    }
}