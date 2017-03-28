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
using static BookRecommender.Models.UserActivity;

namespace BookRecommender.Controllers
{
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

        // GET: /Book/Detail
        public IActionResult Detail(int id)
        {
            string userId = null;
            if(User.Identity.IsAuthenticated){
                userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
            }
            var bookDetail = new BookDetail(id,userId);
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
                var ua = new UserActivity(user,ActivityType.BookDetailViewed,id.ToString());
                db.UsersActivities.Add(ua);
                db.SaveChangesAsync();
            }

            return View(bookDetail);
        }
        [HttpGet]
        public IActionResult AddRating(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            var db = new BookRecommenderContext();
            var book = db.Books.Where(b => b.BookId == id)?.FirstOrDefault();
            if (book == null)
            {
                return View("Error");
            }
            return View(book);
        }
        [HttpPost]
        public IActionResult AddRating(string bookId, string textRating, string scoreRating)
        {
            int bookIdParsed;
            int scoreRatingParsed;
            if(!int.TryParse(bookId, out bookIdParsed) || !int .TryParse(scoreRating, out scoreRatingParsed)){
                return View("Error");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            var db = new BookRecommenderContext();
            var userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
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

            book.AddRating(textRating, scoreRatingParsed, user, db);
            db.SaveChanges();
            return RedirectToAction("Detail","Book", new { id = bookIdParsed});
        }

        // GET: /Book/Review
        public IActionResult Review()
        {
            return View();
        }


        // GET: /Book/Similar
        public IActionResult Similar()
        {
            return View();
        }
    }
}