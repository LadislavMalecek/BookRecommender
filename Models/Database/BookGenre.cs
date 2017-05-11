using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database
{
    public class BookGenre
    {
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }

        public BookGenre(Book book, Genre genre)
        {
            this.BookId = book.BookId;
            this.GenreId = genre.GenreId;
        }
        // EF needs simple constructor
        public BookGenre() { }
    }
}