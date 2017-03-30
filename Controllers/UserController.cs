using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookRecommender.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            string userId = (await _userManager.GetUserAsync(HttpContext.User)).Id;
            var db = new BookRecommenderContext();
            var ratings = db.Ratings.Where(r => r.UserId == userId);
            return View(new MyProfileViewModel(ratings, db));
        }
    }
}
