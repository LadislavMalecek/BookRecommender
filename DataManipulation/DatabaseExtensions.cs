using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BookRecommender.Models;

namespace BookRecommender.DataManipulation{

    public static class DatabaseExtensions{

        static BookRecommenderContext db = new BookRecommenderContext();

        // public static IEnumerable<string> GetNames(this Author author){
        //     return db.Authors.Select(c => c.Name);
        // }
        public static void AddToDb(this Author author){
            db.Authors.Add(author);
            db.SaveChanges();
        }
        public static void AddToDb(this Book book){
            db.Books.Add(book);
            db.SaveChanges();
        }

        // public static void Execute(){
            
        // }




    }
}