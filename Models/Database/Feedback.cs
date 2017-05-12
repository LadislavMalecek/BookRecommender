using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.Database
{
    public class Feedback
    {
        [Required]
        public int FeedbackId { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime CreatedTime { get; private set; }
        public Feedback(string userId, string text, string name)
        {
            UserId = userId;
            Text = text;
            Name = name;
            CreatedTime = DateTime.UtcNow;

        }
    }
}