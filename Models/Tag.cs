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
        public int BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public string Value { get; set; }
        public double? Score { get; set; }

        public Tag(Book book, string value, double? score = null)
        {
            Book = book;
            Value = value;
            Score = score;
        }
        // EF needs simple constructor
        public Tag() { }

        public IEnumerable<(Book book, double? tagScore)> GetBooksAndScoresForSameTag(BookRecommenderContext db)
        {
            return db.Tags.Where(bt => bt.Value == Value).Select(bt => new ValueTuple<Book, double?>(bt.Book, bt.Score));
        }
        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.Tags.Where(bt => bt.Value == Value).Select(t => t.Book);
        }
    }
}