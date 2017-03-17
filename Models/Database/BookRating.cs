using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public class BookRating
    {
        [Required]
        public int BookRatingId { get; protected set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User{ get; set; }

        public string TextRating { get; set; }

        [Required]
        public int Rating { get; set; }

        public BookRating(ApplicationUser user, Book book, int rating, string textRating = null)
        {
            User = user;
            Book = book;
            Rating = rating;
            TextRating = textRating;

        }
        // EF needs simple constructor
        public BookRating() { }

        public Book GetBook(BookRecommenderContext db){
            return db.Books.Where(b => b.BookId == BookId)?.FirstOrDefault();
        }
    }
}