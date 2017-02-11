using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models
{
    public class Book
    {
        [Required]
        public int BookId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string Title { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public string NameOrig { get; set; }
        public string OrigLang { get; set; }
        public string Publisher { get; set; }
        public string ISBN10 { get; set; }
        public string ISBN13 { get; set; }
        public string GndId { get; set; }
        public string OpenLibId { get; set; }
        public string FreeBase { get; set; }

        public string GetNameEn(){
            if(NameEn != null){
                return NameEn;
            }
            if(NameOrig != null){
                return NameOrig;
            }
            if(Title != null){
                return Title;
            }
            if(NameCs != null){
                return NameCs;
            }
            return null;
        }


        [Column("PublicationDate")]
        public string PublicationDateString { get; protected set; }

        [NotMapped]
        public HistoricalDateTime PublicationDate
        {
            get
            {
                return HistoricalDateTime.FromDatabase(PublicationDateString);
            }
            set
            {
                PublicationDateString = value.ToDatabaseString();
            }
        }


        public virtual List<BookAuthor> BooksAuthors { get; protected set; } = new List<BookAuthor>();
        public virtual List<BookGenre> BooksGenres { get; protected set; } = new List<BookGenre>();
        public virtual List<BookCharacter> BooksCharacters { get; protected set; } = new List<BookCharacter>();
        public virtual List<BookTag> BooksTags { get; protected set; } = new List<BookTag>();


        public IEnumerable<Author> GetAuthors(BookRecommenderContext db)
        {
            return db.BooksAuthors.Where(ba => ba.Book == this).Select(ba => ba.Author);
        }
        public IEnumerable<Genre> GetGenres(BookRecommenderContext db)
        {
            return db.BooksGenres.Where(bg => bg.Book == this).Select(bg => bg.Genre);
        }
        public IEnumerable<Character> GetCharacters(BookRecommenderContext db)
        {
            return db.BooksCharacters.Where(bc => bc.Book == this).Select(bc => bc.Character);
        }
        public IEnumerable<Tag> GetTags(BookRecommenderContext db)
        {
            return db.BooksTags.Where(bt => bt.Book == this).Select(bt => bt.Tag);
        }

        public void AddAuthor(Author author, BookRecommenderContext db)
        {
            var newBA = new BookAuthor() { BookId = this.BookId, AuthorId = author.AuthorId };
            if (!db.BooksAuthors.Where(ba => ba.AuthorId == author.AuthorId && ba.BookId == this.BookId).Any())
            {
                db.BooksAuthors.Add(newBA);
            }
        }
        public void AddGenre(Genre genre, BookRecommenderContext db)
        {
            db.BooksGenres.Add(new BookGenre(this, genre));
        }
        public void AddCharacter(Character character, BookRecommenderContext db)
        {
            db.BooksCharacters.Add(new BookCharacter(this, character));
        }
        public void AddTag(Tag tag, BookRecommenderContext db)
        {
            db.BooksTags.Add(new BookTag(this, tag));
        }
    }
}