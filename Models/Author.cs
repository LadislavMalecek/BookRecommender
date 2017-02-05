using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public enum SexType { Male, Female }
    public class Author
    {
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public SexType? Sex { get; set; }


        [Column("DateBirth")]
        public string DateBirthString { get; private set; }

        [NotMapped]
        public HistoricalDateTime DateBirth
        {
            get
            {
                return HistoricalDateTime.FromDatabase(DateBirthString);
            }
            set
            {
                DateBirthString = value.ToDatabaseString();
            }
        }


        [Column("DateDeath")]
        public string DateDeathString { get; private set; }

        [NotMapped]
        public HistoricalDateTime DateDeath
        {
            get
            {
                return HistoricalDateTime.FromDatabase(DateDeathString);
            }
            set
            {
                DateDeathString = value.ToDatabaseString();
            }
        }
        public virtual List<BookAuthor> BooksAuthors { get; set; } = new List<BookAuthor>();

        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.BooksAuthors.Where(ba => ba.Author == this).Select(ba => ba.Book);
        }

    }
}