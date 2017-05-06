using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using static BookRecommender.Models.UserActivity;

namespace BookRecommender.Controllers
{
    public class AuthorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthorController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Author/Detail
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
                var ua = new UserActivity(user, ActivityType.BookDetailViewed, id.ToString());
                await db.UsersActivities.AddAsync(ua);
                await db.SaveChangesAsync();
            }

            return View(new AuthorDetail()
            {
                Author = author,
                Books = authorBooks,
            });
        }



        // GET: /Author/Review
        public IActionResult Review()
        {
            return View();
        }


        // GET: /Author/Similar
        public IActionResult Similar()
        {
            return View();
        }
    }
}