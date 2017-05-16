using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BookRecommender.Models.Database;
using BookRecommender.Models.HomeViewModels;
using static BookRecommender.Models.Database.UserActivity;

namespace BookRecommender.Controllers
{

    /// <summary>
    /// Controller for managing the home page and other general things such as About page and Search page
    /// /Home/
    /// </summary>
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// The main action for the controler and the default route.
        /// </summary>
        /// <returns>Renders the homepage</returns>
        public IActionResult Index()
        {
            ViewData["SignedIn"] = User.Identity.IsAuthenticated;
            return View();
        }

        /// <summary>
        /// Search page shown. Can be show with or without results based on the query parameter.
        /// Supports pagination.
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="page">Page of the search result to be retrieved</param>
        /// <returns>Search page</returns>
        [HttpGet]
        async public Task<IActionResult> Search(string query, int? page)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View();
            }

            // Add default pagination
            if (!page.HasValue)
            {
                page = 1;
            }

            var db = new BookRecommenderContext();
            var books = SearchEngine.SearchBook(db, query);
            var authors = SearchEngine.SearchAuthor(db, query);

            var searchModel = new SearchViewModel(query, page.Value, books.ToList(), authors.ToList(), db);

            // search the searched query if the user is signed in
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


        /// <summary>
        /// About page.
        /// </summary>
        /// <returns>About page</returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View(new AboutViewModel());
        }
    }
}
