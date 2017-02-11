using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;

namespace BookRecommender.Controllers
{
    public class BookController : Controller{

        // GET: /Book/Detail
        public IActionResult Detail(int id){
            var db = new BookRecommenderContext();
            var book = db.Books.Where(b => b.BookId == id)?.FirstOrDefault();
            var bookAuthors = book.GetAuthors(db);

            if(book == null){
                return View("Error");
            }

            return View(new BookDetail(){
                Book = book,
                Authors = bookAuthors
            });
        }


        // GET: /Book/Search  
        [HttpGet]    
        public IActionResult Search(){
            ViewData["method"] = Request.Method.ToString();
            return View();
        }
        [HttpPost]
        public IActionResult Search(Search search){
            ViewData["method"] = Request.Method.ToString();
            ViewData["search"] = search.SearchPhrase;
            
            var db = new BookRecommenderContext();
            var books = db.Books.Where(b => b.NameEn.Contains(search.SearchPhrase));
            var authors = db.Authors.Where(a => a.NameEn.Contains(search.SearchPhrase));
            
            search.BooksFound = books;
            search.AuthorsFound = authors;

            return View(search);
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