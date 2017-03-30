using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using static BookRecommender.Models.UserActivity;
using System.Threading.Tasks;

namespace BookRecommender.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Users()
        {
            var db = new BookRecommenderContext();
            var users = db.Users.ToList();
            ViewData["CurrentName"] = User.Identity.Name;
            return View(users);
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Book/Search  
        [HttpGet]
        async public Task<IActionResult> Search(string query, int? page)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View();
            }

            if (!page.HasValue)
            {
                page = 1;
            }

            var db = new BookRecommenderContext();
            var books = SearchEngine.SearchBook(db, query);
            var authors = SearchEngine.SearchAuthor(db, query);

            var searchModel = new Search(query, page.Value, books.ToList(), authors.ToList(), db);

            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                var user = db.Users.Where(u => u.Id == userId)?.FirstOrDefault();
                if (user == null)
                {
                    return View("Error");
                }
                var ua = new UserActivity(user,ActivityType.KeywordSearched,query);
                await db.UsersActivities.AddAsync(ua);
                await db.SaveChangesAsync();
            }
            return View(searchModel);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
