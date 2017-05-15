
using System.Linq;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.HomeViewModels
{
    public class AboutViewModel
    {
        public int BooksCount;
        public int AuthorsCount;
        public int ReviewsCount;
        public int RatingsCount;
        public int UsersCount;

        public AboutViewModel(){
            var db = new BookRecommenderContext();

            BooksCount = db.Books.Count();
            AuthorsCount = db.Authors.Count();
            ReviewsCount = db.Ratings.Where(r => !string.IsNullOrEmpty(r.Review)).Count();
            RatingsCount = db.Ratings.Count();
            UsersCount = db.Users.Count();
        }
    }
}