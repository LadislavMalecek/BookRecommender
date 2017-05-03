using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookRecommender.Models
{
    public class Book : IGoogleImg
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
        public string OriginalImage { get; set; }
        public string GoogleImageCache { get; set; }
        public string Description { get; set; }
        public string WikipediaPage { get; set; }

        public string GetNameEn()
        {
            if (!string.IsNullOrEmpty(NameEn))
            {
                return NameEn;
            }
            if (!string.IsNullOrEmpty(NameOrig))
            {
                return NameOrig;
            }
            if (!string.IsNullOrEmpty(Title))
            {
                return Title;
            }
            if (!string.IsNullOrEmpty(NameCs))
            {
                return NameCs;
            }
            return null;
        }

        public string GetNameEnShort(int numberOfChars, string afterChars = "...")
        {
            var name = GetNameEn();
            if (name.Length <= numberOfChars)
            {
                return name;
            }
            else
            {
                return name.Substring(0,numberOfChars) + afterChars;
            }
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

        // m : n relationships
        public virtual List<BookAuthor> BooksAuthors { get; protected set; } = new List<BookAuthor>();
        public virtual List<BookGenre> BooksGenres { get; protected set; } = new List<BookGenre>();
        public virtual List<BookCharacter> BooksCharacters { get; protected set; } = new List<BookCharacter>();

        // 1 : n relationships
        public virtual List<Tag> Tags { get; protected set; } = new List<Tag>();
        public virtual List<BookRating> Ratings { get; protected set; } = new List<BookRating>();


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
        public IEnumerable<Tag> GetTags(BookRecommenderContext db, string lang = null)
        {
            if (lang != null)
            {
                return db.Tags.Where(bt => bt.Book == this).Where(t => t.Lang == "en");
            }
            return db.Tags.Where(bt => bt.Book == this);
        }
        public IEnumerable<BookRating> GetRatings(BookRecommenderContext db)
        {
            return db.Ratings.Where(br => br.Book == this);
        }
        public int? GetRating(BookRecommenderContext db)
        {
            var ratings = GetRatings(db);
            var count = 0;
            var sum = ratings.Select(r => r.Rating).Sum(r => { count++; return r; });
            if (count == 0)
            {
                return null;
            }
            return (int)(sum / count);
        }
        public int? GetPreciseRating(BookRecommenderContext db)
        {
            var ratings = GetRatings(db);
            var count = 0;
            var sum = ratings.Select(r => r.Rating).Sum(r => { count++; return r; });
            if (count == 0)
            {
                return null;
            }
            return (int)(sum * 20 / count);
        }


        public void AddAuthor(Author author, BookRecommenderContext db)
        {
            AddAuthorAsync(author, db).Wait();
        }
        public async Task AddAuthorAsync(Author author, BookRecommenderContext db)
        {
            var newBA = new BookAuthor() { BookId = this.BookId, AuthorId = author.AuthorId };
            if (!db.BooksAuthors.Where(ba => ba.AuthorId == author.AuthorId && ba.BookId == this.BookId).Any())
            {
                await db.BooksAuthors.AddAsync(newBA);
            }
        }


        public void AddGenre(Genre genre, BookRecommenderContext db)
        {
            AddGenreAsync(genre, db).Wait();
        }
        public async Task AddGenreAsync(Genre genre, BookRecommenderContext db)
        {
            await db.BooksGenres.AddAsync(new BookGenre(this, genre));
        }


        public void AddCharacter(Character character, BookRecommenderContext db)
        {
            AddCharacterAsync(character, db).Wait();
        }
        public async Task AddCharacterAsync(Character character, BookRecommenderContext db)
        {
            await db.BooksCharacters.AddAsync(new BookCharacter(this, character));
        }


        public void AddTag(Tag tag, BookRecommenderContext db)
        {
            AddTagAsync(tag, db).Wait();
        }
        public async Task AddTagAsync(Tag tag, BookRecommenderContext db)
        {
            await db.Tags.AddAsync(new Tag(this, tag.Value, tag.Lang, tag.Score));
        }


        public void AddRating(BookRating rating, ApplicationUser user, BookRecommenderContext db)
        {
            AddRatingAsync(rating, user, db).Wait();
        }
        public async Task AddRatingAsync(BookRating rating, ApplicationUser user, BookRecommenderContext db)
        {
            await db.Ratings.AddAsync(new BookRating(user, this, rating.Rating, rating.TextRating));
        }


        public void AddRating(string textRating, int rating, ApplicationUser user, BookRecommenderContext db)
        {
            AddRatingAsync(textRating, rating, user, db).Wait();
        }
        public async Task AddRatingAsync(string textRating, int rating, ApplicationUser user, BookRecommenderContext db)
        {
            await db.Ratings.AddAsync(new BookRating(user, this, rating, textRating));
        }


        public async Task<string> TryToGetImgUrlAsync()
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
                    pictureUrl = await imageMiner.GetFirstImageUrlAsync("book " + GetNameEn());
                    // Save to cache
                    if (pictureUrl != null)
                    {
                        GoogleImageCache = pictureUrl;
                        var db = new BookRecommenderContext();
                        db.Books.Update(this);
                        await db.SaveChangesAsync();
                    }
                }
            }
            return pictureUrl;
        }
    }
}