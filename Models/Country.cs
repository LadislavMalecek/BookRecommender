using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models{
    public class Country {
        [Required]
        public int CountryId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string Name_en { get; set; }
        public string Name_cs { get; set; }
        //public ICollection<Author> AuthorsWithCitizenship { get; set; }
    }
}