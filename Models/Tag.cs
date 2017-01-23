using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models{

    public enum Language { en, cs }
    public class Tag {
        [Required]
        public int TagId { get; set; }
        public Language Language { get; set; }
        [Required]
        public string Value { get; set; }

        virtual public ICollection<BookTag> BookTag { get; set; }
    }
}