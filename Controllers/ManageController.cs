using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models.ManageViewModels;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BookRecommender.Controllers
{

    /// <summary>
    /// Controller that handles all manage pages
    /// Only available to signed in users
    /// /Manage/
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ManageController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Main manage page action, if user does not have an Admin access,
        /// he will be redirected to get access action.  
        /// </summary>
        /// <returns>Manage page or Redirect</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }

            bool hasAccess = (await _userManager.GetUserAsync(HttpContext.User)).HasManageAccess;
            if(!hasAccess){
                return Redirect("/Manage/GetAccess");
            }
            var db = new BookRecommenderContext();
            return View(new IndexViewModel());
        }

        /// <summary>
        /// Get access to manage page action
        /// </summary>
        /// <returns>Get access page</returns>
        [HttpGet]
        public IActionResult GetAccess(){
            return View();
        }

        /// <summary>
        /// Evaluation of access request. If password successful, the access will be saved to the database.
        /// for later automatic aproval.
        /// </summary>
        /// <param name="model">Model with access request information</param>
        /// <returns>Redirect to Manage page or prompt to reinsert the password</returns>
        [HttpPost]
        public async Task<IActionResult> GetAccess(GetAccessViewModel model){
            if (!User.Identity.IsAuthenticated)
            {
                return View("Error");
            }
            if(model.Password == AppSettingsSingleton.Mining.Password){
                string userId = (await _userManager.GetUserAsync(HttpContext.User)).Id;
                var db = new BookRecommenderContext();
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if(user != null){
                    user.HasManageAccess = true;
                    db.SaveChanges();
                }
                return Redirect("/Manage");
            } else{
                ModelState.AddModelError(string.Empty, "Invalid access attempt.");
                return View();
            }
        }
    }
}