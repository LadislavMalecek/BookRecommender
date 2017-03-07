using BookRecommender.DataManipulation;
using System.Collections.Generic;

namespace BookRecommender.Models
{
    public class BookDetail
    {
        public Book Book { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<Character> Characters { get; set; }
        public AdditionalSparqlData AdditionalData { get; set; }
    }
}