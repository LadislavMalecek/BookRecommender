using System;
using System.ComponentModel.DataAnnotations;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.Database
{
    public class UserActivity
    {
        public enum ActivityType
        {
            KeywordSearched, BookDetailViewed, AuthorDetailViewed
        }
        [Required]
        public int UserActivityId { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public ActivityType Type { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public DateTime CreatedTime { get; private set; }

        public UserActivity(ApplicationUser user, ActivityType type, string value)
        {
            User = user;
            Type = type;
            Value = value;
            CreatedTime = DateTime.UtcNow;
        }
        // EF needs simple constructor
        public UserActivity() { }
    }
}