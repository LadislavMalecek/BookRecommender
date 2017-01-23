using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database{
    public class BookAuthor {
        public int BookAuthorId { get; set; }
        public Book Book;
        public Author Author;
    }
}