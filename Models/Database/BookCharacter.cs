using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database
{
    public class BookCharacter
    {
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int CharacterId { get; set; }
        public Character Character { get; set; }

        public BookCharacter(Book book, Character character)
        {
            this.Book = book;
            this.Character = character;
        }
        // EF needs simple constructor
        public BookCharacter() { }
    }
}