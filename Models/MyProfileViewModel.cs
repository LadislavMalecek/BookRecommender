using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models
{
    public class MyProfileViewModel
    {
        public List<(BookRating rating, Book book)> BookRatings = new List<(BookRating rating, Book book)>();

        public MyProfileViewModel(IEnumerable<BookRating> ratings, BookRecommenderContext db){
            foreach(var rating in ratings){
                BookRatings.Add((rating, rating.GetBook(db)));
            }
        }
    }
}
