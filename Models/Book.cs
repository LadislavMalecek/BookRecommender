using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookRecommender.Models.Database;

namespace BookRecommender.Models{

    public class Book{

        [Required]
        public int BookId{ get; set; }
        [Required]
        public string Uri { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public string NameOrig { get; set; }
        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
        public virtual ICollection<BookGenre> Genre { get; set; }
        public string OrigLang { get; set; }
        public virtual ICollection<Character> Characters { get; set; }
        public string Title { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Publisher { get; set; }
        public string ISBN10 { get; set; }
        public string ISBN13 { get; set; }
        public string GndId { get; set; }
        public string OpenLibId { get; set; }
        public string FreeBase { get; set; }
        public virtual ICollection<BookTag> Tags { get; set; }
    }
}