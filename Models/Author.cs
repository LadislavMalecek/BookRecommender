using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public class Author
    {
        [Required]
        public int AuthorId{ get; set; }
        [Required]
        public string URI { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public DateTime? DateBirth { get; set; }
        public DateTime? DateDeath { get; set; }

        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
        public virtual ICollection<Country> CountryCitizenship { get; set; }

        // public Author()
        // {
        //     CountryCitizenship = new List<Country>();
        //     Books = new List<Book>();
        // }
    }
}