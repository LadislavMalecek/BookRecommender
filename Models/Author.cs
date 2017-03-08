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
    public class Author : IGoogleImg
    {
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public string Uri { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string NameCs { get; set; }
        public SexType? Sex { get; set; }

        public string OriginalImage { get; set; }
        public string GoogleImageCache { get; set; }
        public string Description { get; set; }
        public string WikipediaPage { get; set; }

        public string GetNameEn(){
            if(NameEn != null){
                return NameEn;
            }
            if(Name != null){
                return Name;
            }
            if(NameCs != null){
                return NameCs;
            }
            return null;
        }


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
                DateBirthString = value?.ToDatabaseString();
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
                DateDeathString = value?.ToDatabaseString();
            }
        }
        public virtual List<BookAuthor> BooksAuthors { get; set; } = new List<BookAuthor>();

        public IEnumerable<Book> GetBooks(BookRecommenderContext db)
        {
            return db.BooksAuthors.Where(ba => ba.Author == this).Select(ba => ba.Book);
        }
        public string TryToGetImgUrl()
        {
            var pictureUrl = OriginalImage;
            if (pictureUrl == null)
            {
                // If no picture from Open Data, then try to load first picture on google search
                // First check cache:
                pictureUrl = GoogleImageCache;
                if (string.IsNullOrEmpty(pictureUrl))
                {
                    // If nothing in cache, try to load from Google
                    var imageMiner = new DataManipulation.GoogleImageMiner();
                    pictureUrl = imageMiner.GetFirstImageUrl("author " + NameEn);
                    // Save to cache
                    if(pictureUrl != null){
                        GoogleImageCache = pictureUrl;
                        var db = new BookRecommenderContext();
                        db.Authors.Update(this);
                        db.SaveChanges();
                    }
                }
            }
            return pictureUrl;
        }
    }
}