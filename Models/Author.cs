using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookRecommender.Models.Database;

namespace BookRecommender.Models
{
    public class Author
    {
        public enum SexType { Male, Female }
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public SexType Sex { get; set; }


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


        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
        public virtual ICollection<Country> CountryCitizenship { get; set; }

        // public Author()
        // {
        //     CountryCitizenship = new List<Country>();
        //     Books = new List<Book>();
        // }
    }
}