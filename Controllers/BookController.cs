using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;

namespace BookRecommender.Controllers
{
    public class BookController : Controller{

        // GET: /Book/Detail
        public IActionResult Detail(){
            var Book = new Book(){
                Name = "My book"
            };
            return View(Book);
        }


        // GET: /Book/Search  
        [HttpGet]    
        public IActionResult Search(){
            ViewData["method"] = Request.Method.ToString();
            
            return View();
        }
        [HttpPost]
        public IActionResult Search(SearchModel search){
            ViewData["method"] = Request.Method.ToString();
            ViewData["search"] = search.SearchPhrase; 
            return View();
        }

        // GET: /Book/Review
        public IActionResult Review(){
            return View();
        }


        // GET: /Book/Similar
        public IActionResult Similar(){
            return View();
        }
    }
}