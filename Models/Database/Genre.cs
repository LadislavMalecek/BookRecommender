using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.Database
{
    public class Genre
    {
        [Required]
        public int GenreId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }

        virtual public List<BookGenre> BooksGenres { get; set; } = new List<BookGenre>();

        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.BooksGenres.Where(bg => bg.Genre == this).Select(bg => bg.Book);
        }
        public IEnumerable<int> GetBooksIds(BookRecommenderContext db)
        {
            return db.BooksGenres.Where(bg => bg.Genre == this).Select(bg => bg.BookId);
        }
    }
}