using BookRecommender.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models{
    public class BookRecommenderContext : DbContext{
        public BookRecommenderContext(DbContextOptions<BookRecommenderContext> options) : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
    }
}