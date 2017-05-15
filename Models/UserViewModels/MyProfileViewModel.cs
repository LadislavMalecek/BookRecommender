using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Models.UserViewModels
{
    public class MyProfileViewModel
    {
        public List<BookRating> BookRatings = new List<BookRating>();
        public List<string> LastSearched = new List<string>();

        public List<Book> LastViewedBooks = new List<Book>();
        public List<Author> LastViewedAuthors = new List<Author>();
        public MyProfileViewModel(string userId)
        {
            var db = new BookRecommenderContext();
            BookRatings = db.Ratings.Where(r => r.UserId == userId)
                                    .OrderByDescending(r => r.CreatedTime)
                                    .Include(x => x.Book)
                                    .ToList();

            var usersActivities = db.UsersActivities
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedTime)
                    .ToList();

            LastSearched = usersActivities
                    .Where(a =>
                        a.Type == UserActivity.ActivityType.KeywordSearched)
                    .Select(a => a.Value)
                    .Distinct()
                    .Take(20)
                    .ToList();

            var lastBooks = usersActivities
                    .Where(a => a.Type == UserActivity.ActivityType.BookDetailViewed)
                    .Select(a => int.Parse(a.Value))
                    .Distinct()
                    .Take(20)
                    .ToList();

            var lastAuthors = usersActivities
                    .Where(a => a.Type == UserActivity.ActivityType.AuthorDetailViewed)
                    .Select(a => int.Parse(a.Value))
                    .Distinct()
                    .Take(20)
                    .ToList();

            foreach(var bookId in lastBooks){
                var book = db.Books.Where(b => b.BookId == bookId).FirstOrDefault();
                if(book != null){
                    LastViewedBooks.Add(book);
                }
            }

            foreach(var authorId in lastAuthors){
                var author = db.Authors.Where(b => b.AuthorId == authorId).FirstOrDefault();
                if(author != null){
                    LastViewedAuthors.Add(author);
                }
            }
        }
    }
}
