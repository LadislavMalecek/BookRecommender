using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
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
    }
}