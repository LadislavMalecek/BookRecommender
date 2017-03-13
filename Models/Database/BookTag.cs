using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database
{
    public class BookTag
    {
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }

        public double? Score { get; set; }

        public BookTag(Book book, Tag tag, double? score = null)
        {
            this.Book = book;
            this.Tag = tag;
            this.Score = score;
        }
        // EF needs simple constructor
        public BookTag() { }
    }
}