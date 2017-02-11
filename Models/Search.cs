using System;
using System.Collections.Generic;

namespace BookRecommender.Models
{
    public class Search
    {
        public string SearchPhrase { get; set; }
        public IEnumerable<Book> BooksFound { get; set; }

        public IEnumerable<Author> AuthorsFound { get; set; }
    }
}