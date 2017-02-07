using System;
using System.Collections.Generic;

namespace BookRecommender.Models
{
    public class Search
    {
        public string SearchPhrase { get; set; }
        public List<string> BooksFound { get; set; } = new List<string>();

        public List<string> AuthorsFound { get; set; } = new List<string>();
    }
}