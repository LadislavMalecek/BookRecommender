using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BookRecommender.Models.Database
{
    public class ApplicationUser : IdentityUser
    {
        public bool HasManageAccess { get; set; } = false;
        public virtual List<BookRating> Ratings { get; protected set; } = new List<BookRating>();
        public IEnumerable<BookRating> GetRatings(BookRecommenderContext db)
        {
            return db.Ratings.Where(br => br.UserId == this.Id);
        }
        public virtual List<UserActivity> Activities { get; protected set; } = new List<UserActivity>();
        public IEnumerable<UserActivity> GetActivities(BookRecommenderContext db)
        {
            return db.UsersActivities.Where(ua => ua.UserId == this.Id);
        }
    }
}