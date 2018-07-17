using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.Database
{
    public class TagCount
    {
        [Required]
        public int TagCountId { get; protected set; }

        [Required]
        
        public string Lang { get; set; }
        [Required]
        public int Count { get; set; }
        public TagCount(string lang, int count)
        {
            this.Lang = lang;
            this.Count = count;
        }
        // EF needs simple constructor
        public TagCount() { }
    }
}