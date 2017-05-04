using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BookRecommender.Models;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookRecommender.DataManipulation
{
    public class BookRecommenderContext : IdentityDbContext<ApplicationUser>
    {
        //public BookRecommenderContext(DbContextOptions<BookRecommenderContext> options) : base(options)
        public BookRecommenderContext() : base()
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagCount> TagsCount { get; set; }
        public DbSet<BookRating> Ratings { get; set; }

        public DbSet<BookAuthor> BooksAuthors { get; set; }
        public DbSet<BookCharacter> BooksCharacters { get; set; }
        public DbSet<BookGenre> BooksGenres { get; set; }
        public DbSet<UserActivity> UsersActivities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlite(AppSettingsSingleton.DataBaseConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //--------------------------------
            // Indexes
            //--------------------------------
            modelBuilder.Entity<Tag>()
                .HasIndex(c => c.Value);

            modelBuilder.Entity<TagCount>()
                .HasIndex(c => c.Lang)
                .IsUnique(true);

            modelBuilder.Entity<BookRating>()
                .HasIndex(c => c.BookId);

            modelBuilder.Entity<BookRating>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<UserActivity>()
                .HasIndex(c => c.UserId);


            //--------------------------------
            // Alternative keys
            //--------------------------------

            modelBuilder.Entity<Book>()
                .HasAlternateKey(c => c.Uri)
                .HasName("AlternateKey_Uri");

            modelBuilder.Entity<Author>()
                .HasAlternateKey(c => c.Uri)
                .HasName("AlternateKey_Uri");

            modelBuilder.Entity<Character>()
                .HasAlternateKey(c => c.Uri)
                .HasName("AlternateKey_Uri");

            modelBuilder.Entity<Genre>()
                .HasAlternateKey(c => c.Uri)
                .HasName("AlternateKey_Uri");

            //--------------------------------
            // Many to many relationships
            //--------------------------------

            modelBuilder.Entity<BookAuthor>()
                .HasKey(t => new { t.BookId, t.AuthorId });

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BooksAuthors)
                .HasForeignKey(ba => ba.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BooksAuthors)
                .HasForeignKey(ba => ba.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            //--------------------------------

            modelBuilder.Entity<BookCharacter>()
                .HasKey(t => new { t.BookId, t.CharacterId });

            modelBuilder.Entity<BookCharacter>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BooksCharacters)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookCharacter>()
                .HasOne(bc => bc.Character)
                .WithMany(c => c.BooksCharacters)
                .HasForeignKey(bc => bc.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            //--------------------------------

            modelBuilder.Entity<BookGenre>()
                .HasKey(t => new { t.BookId, t.GenreId });

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BooksGenres)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BooksGenres)
                .HasForeignKey(bg => bg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            //--------------------------------
            // One to many relationships
            //--------------------------------

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Book)
                .WithMany(t => t.Tags)
                .HasForeignKey(t => t.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookRating>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Ratings)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookRating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserActivity>()
                .HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}