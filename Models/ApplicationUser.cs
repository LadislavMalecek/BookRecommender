using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BookRecommender.Models
{
    public class ApplicationUser : IdentityUser {
        public virtual List<BookRating> Ratings { get; protected set; } = new List<BookRating>();
    }
}