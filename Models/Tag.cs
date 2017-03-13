using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public enum Language { en, cs }
    public class Tag
    {
        [Required]
        public int TagId { get; protected set; }
        public Language Language { get; set; }
        [Required]
        public string Value { get; set; }
        
        virtual public List<BookTag> BooksTags { get; set; } = new List<BookTag>();
        public IEnumerable<(Book book, double? tagScore)> GetBooksAndScores(BookRecommenderContext db)
        {
            return db.BooksTags.Where(bt => bt.Tag == this).Select(bt => new ValueTuple<Book, double?>(bt.Book, bt.Score));
        }
        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.BooksTags.Where(bt => bt.Tag == this).Select(bt => bt.Book);
        }
    }
}