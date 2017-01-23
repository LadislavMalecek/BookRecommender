using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models{
    public class Character {
        public int CharacterId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public string NameOrig { get; set; }
        public virtual ICollection<BookCharacter> BooksWhereIn { get; set; }
    }
}