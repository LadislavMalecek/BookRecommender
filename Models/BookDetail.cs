
using System.Collections.Generic;

namespace BookRecommender.Models{
    public class BookDetail{
        public Book Book { get; set; }
        public IEnumerable<Author> Authors { get; set; }

    }
}