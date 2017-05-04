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
        public ApplicationUser User { get; set; }

        public string Review { get; set; }

        [Required]
        public int Rating { get; set; }

        [Required]
        public DateTime CreatedTime { get; private set; }

        public BookRating(ApplicationUser user, Book book, int rating, string review = null)
        {
            if(rating < 0 || rating > 5){
                throw new ArgumentOutOfRangeException("Rating should be between zero and five, it was: " + rating);
            }
            User = user;
            Book = book;
            Rating = rating;
            Review = review;
            CreatedTime = DateTime.UtcNow;

        }
        // EF needs simple constructor
        public BookRating() { }

        public Book GetBook(BookRecommenderContext db)
        {
            return db.Books.Where(b => b.BookId == BookId)?.FirstOrDefault();
        }
    }
}