using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public class Character
    {
        [Required]
        public int CharacterId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public virtual List<BookCharacter> BooksCharacters { get; protected set; } = new List<BookCharacter>();

        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.BooksCharacters.Where(bc => bc.Character == this).Select(bc => bc.Book);
        }
    }
}