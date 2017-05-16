using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BookRecommender.Models.Database;
using BookRecommender.Models.BookViewModels;
using static BookRecommender.Models.Database.UserActivity;

namespace BookRecommender.Controllers
{
    /// <summary>
    /// Controller for handling pages about books
    /// /Book/
    /// </summary>
    public class BookController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;

        public BookController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        /// <summary>
        /// Detail of the book.
        /// </summary>
        /// <param name="id">DId of the book</param>
        /// <returns>Page with the main detail page</returns>
        async public Task<IActionResult> Detail(int id)
        {
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = (await _userManager.GetUserAsync(HttpContext.User)).Id;
            }
            var bookDetail = new BookDetail(id, userId);
            if (bookDetail.Book == null)
            {
                return View("Error");
            }

            if (User.Identity.IsAuthenticated)
            {
                var db = new BookRecommenderContext();
                var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
                if (user == null)
                {
                    return View("Error");
                }
                var ua = new UserActivity(user, ActivityType.BookDetailViewed, id.ToString());
                await db.UsersActivities.AddAsync(ua);
                await db.SaveChangesAsync();
            }

            return View(bookDetail);
        }


        /// <summary>
        /// Method to validate and add a book rating from the user
        /// </summary>
        /// <param name="bookId">Id of the book</param>
        /// <param name="review">Review written by the user</param>
        /// <param name="scoreRating">Score awarded by the user</param>
        /// <returns>Result of the rating</returns>
        [HttpPost]
        async public Task<IActionResult> AddRating(string bookId, string review, string scoreRating)
        {
            int bookIdParsed;
            int scoreRatingParsed;
            if (!int.TryParse(bookId, out bookIdParsed) || !int.TryParse(scoreRating, out scoreRatingParsed))
            {
                return View("Error");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            var db = new BookRecommenderContext();
            var userId = (await _userManager.GetUserAsync(HttpContext.User)).Id;
            var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
            if (user == null)
            {
                return View("Error");
            }
            var book = db.Books.Where(b => b.BookId == bookIdParsed)?.FirstOrDefault();
            if (book == null)
            {
                return View("Error");
            }
            var rating = db.Ratings.Where(r => r.BookId == book.BookId && r.UserId == user.Id).FirstOrDefault();

            if (rating == null)
            {
                await book.AddRatingAsync(review, scoreRatingParsed, user, db);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Detail", "Book", new { id = bookIdParsed });

        }
    }
}