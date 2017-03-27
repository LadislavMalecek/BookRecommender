using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;

namespace BookRecommender.Controllers
{
    public class AuthorController : Controller
    {

        // GET: /Author/Detail
        public IActionResult Detail(int id)
        {
            var db = new BookRecommenderContext();
            var author = db.Authors.Where(a => a.AuthorId == id)?.FirstOrDefault();
            var authorBooks = author.GetBooks(db);

            if (author == null)
            {
                return View("Error");
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