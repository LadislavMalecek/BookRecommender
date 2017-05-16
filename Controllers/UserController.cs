using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BookRecommender.Models.Database;
using BookRecommender.Models.UserViewModels;

namespace BookRecommender.Controllers
{
    /// <summary>
    /// Controller that handles all user related actions
    /// /User/
    /// </summary>
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// /User/MyProfile, page with a user specific information.
        /// </summary>
        /// <returns>Returns users profile page</returns>
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            string userId = (await _userManager.GetUserAsync(HttpContext.User)).Id;
            return View(new MyProfileViewModel(userId));
        }
    }
}
