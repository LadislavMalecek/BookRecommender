using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database{
    public class BookCharacter {
        public int BookCharacterID { get; set; }
        public Book Book;
        public Character Character;
    }
}