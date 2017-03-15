using BookRecommender.DataManipulation;
using System.Collections.Generic;
using System.Linq;

namespace BookRecommender.Models
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
                Tags = Book.GetTags(db);

                var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
                if (user != null)
                {
                    SignedIn = true;
                    BookRating = db.Ratings.Where(r => r.BookId == Book.BookId && r.UserId == userId)?.FirstOrDefault();
                }
            }
        }
        public Book Book { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<Character> Characters { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public bool SignedIn { get; set; }
        public BookRating BookRating { get; set; }
    }
}