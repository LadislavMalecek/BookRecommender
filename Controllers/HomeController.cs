using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;

namespace BookRecommender.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Users(){
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
        public IActionResult Search(string query, int? page)
        {
            if(string.IsNullOrEmpty(query)){
                return View();
            }

            if (!page.HasValue)
            {
                page = 1;
            }

            var db = new BookRecommenderContext();
            var books = db.Books.Where(b => b.NameEn.Contains(query));
            var authors = db.Authors.Where(a => a.NameEn.Contains(query));

            var searchModel = new Search(query, page.Value, books.ToList(), authors.ToList());

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
