using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.Database
{
    public class BookRecommenderContext : DbContext{
//        public BookRecommenderContext(DbContextOptions<BookRecommenderContext> options) : base(options)
        public BookRecommenderContext() : base()
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder){
            optionBuilder.UseSqlite("Filename=C://netcore//SQLite//BookRecommender.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            // modelBuilder.Entity<BookAuthor>()
            // .HasKey(c => c.BookAuthorId);


            // modelBuilder.Entity<Book>()
            // .HasKey(c => c.BookID);

            // modelBuilder.Entity<Book>()
            // .HasIndex(c => c.URI);



            // modelBuilder.Entity<Author>()
            // .HasKey(c => c.AuthorID);

            // modelBuilder.Entity<Author>()
            // .HasIndex(c => c.URI);
            }
    }
}