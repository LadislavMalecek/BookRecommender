using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BookRecommender.Models
{
    public class ApplicationUser : IdentityUser {
        public virtual List<BookRating> Ratings { get; protected set; } = new List<BookRating>();
        public IEnumerable<BookRating> GetRatings(BookRecommenderContext db)
        {
            return db.Ratings.Where(br => br.User == this);
        }
    }
}