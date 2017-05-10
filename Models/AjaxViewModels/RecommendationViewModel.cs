using BookRecommender.DataManipulation;
using System.Collections.Generic;
using BookRecommender.Models.Database;
using System.Linq;

namespace BookRecommender.Models.AjaxViewModels
{
    public class Recommendation
    {
        public Recommendation(int bookId, string reason = null)
        {
            var db = new BookRecommenderContext();
            Book = db.Books.Where(b => b.BookId == bookId)?.FirstOrDefault();
            if (Book != null)
            {
                Authors = Book.GetAuthors(db);
                Genres = Book.GetGenres(db);
                RecommendationReason = reason;
            }
        }
        public Book Book { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public string RecommendationReason { get; set; }
    }
}