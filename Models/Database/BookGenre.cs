using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database{
    public class BookGenre {
        public int BookGenreId { get; set; }
        public Book Book;
        public Genre Genre;
    }
}