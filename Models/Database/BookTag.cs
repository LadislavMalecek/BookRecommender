using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database{
    public class BookTag {
        public int BookTagId { get; set; }
        public Book Book;
        public Tag Tag;
    }
}