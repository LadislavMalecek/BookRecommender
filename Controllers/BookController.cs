using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;

namespace BookRecommender.Controllers
{
    public class BookController : Controller
    {
        // GET: /Book/Detail
        public IActionResult Detail(int id)
        {
            var db = new BookRecommenderContext();
            var book = db.Books.Where(b => b.BookId == id)?.FirstOrDefault();
            var bookAuthors = book.GetAuthors(db);
            var bookGenres = book.GetGenres(db);
            var bookCharacters = book.GetCharacters(db);
            var additionalData = MineSPARQL.GetAdditionalData(book.Uri);

            if (book == null)
            {
                return View("Error");
            }

            return View(new BookDetail()
            {
                Book = book,
                Authors = bookAuthors,
                Genres = bookGenres,
                Characters = bookCharacters,
                AdditionalData = additionalData
            });
        }

        // GET: /Book/Review
        public IActionResult Review()
        {
            return View();
        }


        // GET: /Book/Similar
        public IActionResult Similar()
        {
            return View();
        }
    }
}