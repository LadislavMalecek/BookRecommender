using BookRecommender.DataManipulation;
using System.Collections.Generic;
using BookRecommender.Models.Database;
using System.Linq;

namespace BookRecommender.Models.BookViewModels
{
    public class BookDetail
    {
        public BookDetail(int bookId, string userId)
        {
            var db = new BookRecommenderContext();
            Book = db.Books.Where(b => b.BookId == bookId)?.FirstOrDefault();
            if (Book != null)
            {
                Authors = Book.GetAuthors(db);
                Genres = Book.GetGenres(db);
                Characters = Book.GetCharacters(db);
                Tags = Book.GetTags(db, "en");
                RecommendedBooks = db.Books.Take(5);
                

                if (userId != null)
                {
                    var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
                    if (user != null)
                    {
                        SignedIn = true;
                        UserBookRating = db.Ratings.Where(r => r.BookId == Book.BookId && r.UserId == userId)?.FirstOrDefault();
                    }
                }
                BookRating = Book.GetPreciseRating(db);
                Ratings = db.Ratings.Where(r => r.BookId == Book.BookId && r.Review != null).OrderByDescending(r => r.CreatedTime);
                

            }
        }
        public Book Book { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<Character> Characters { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public bool SignedIn { get; set; }
        public int? BookRating { get; set; }
        public BookRating UserBookRating { get; set; }
        public IEnumerable<BookRating> Ratings { get; set; }
        public IEnumerable<Book> RecommendedBooks { get; set; }
    }
}