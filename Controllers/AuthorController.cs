using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using BookRecommender.Models.Database;
using static BookRecommender.Models.Database.UserActivity;
using BookRecommender.Models.AuthorViewModels;

namespace BookRecommender.Controllers
{

    /// <summary>
    /// Controller for handling pages about authors
    /// /Author/
    /// </summary>
    public class AuthorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthorController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Author/Detail
        /// <summary>
        /// Gets the main page for the autor
        /// </summary>
        /// <param name="id">Id of the author</param>
        /// <returns>Page with na author detail</returns>
        async public Task<IActionResult> Detail(int id)
        {
            var db = new BookRecommenderContext();
            var author = db.Authors.Where(a => a.AuthorId == id)?.FirstOrDefault();
            var authorBooks = author.GetBooks(db).OrderBy(b => b.GetNameEn());

            if (author == null)
            {
                return View("Error");
            }

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
                if (user == null)
                {
                    return View("Error");
                }
                var ua = new UserActivity(user, ActivityType.AuthorDetailViewed, id.ToString());
                await db.UsersActivities.AddAsync(ua);
                await db.SaveChangesAsync();
            }

            return View(new AuthorDetail()
            {
                Author = author,
                Books = authorBooks,
            });
        }
    }
}