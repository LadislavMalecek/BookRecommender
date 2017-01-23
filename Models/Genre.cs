using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models{
    public class Genre {
        [Required]
        public int GenreId { get; set;}
        [Required]
        public string Uri { get; set; }
        public string Name_en { get; set; }
        public string Name_cs { get; set; }

        virtual public ICollection<BookGenre> BookWithThisGenre { get; set; }
    }
}