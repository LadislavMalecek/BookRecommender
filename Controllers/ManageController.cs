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
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ManageController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


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

        [HttpGet]
        public IActionResult GetAccess(){
            return View();
        }


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