using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BookRecommender.Models;
using BookRecommender.Models.Database;
using System.Diagnostics;
using BookRecommender.DataManipulation.WikiData;

namespace BookRecommender.DataManipulation
{

    public class MineSPARQL
    {
        // static string ShortenUri(string uri){
        //     return uri.Substring(uri.LastIndexOf("/") + 1);
        // }
        // static int UriToNumber(string uri){
        //     return int.Parse(uri.Substring(uri.LastIndexOf("/") + 2));
        // }
        public static void Mine()
        {
            var endpoint = new WikiDataEndpointMiner();

            // using(var db = new BookRecommenderContext()){
            //     var counter = new Counter(db.Books.Count());

            //     foreach(var book in db.Books){
            //         System.Console.Write(counter);
            //         db.Books.Remove(book);
            //         counter++;
            //     }

            //     db.SaveChanges();
            // }

            // System.Console.WriteLine("All books removed");


            System.Console.WriteLine("Update book uris");
            using (var db = new BookRecommenderContext())
            {

                var allBookUris = endpoint.GetBooks_Uri().ToList();

                //Create new console progress counter
                var counter = new Counter(allBookUris.Count);

                //Insert all books in database
                foreach (var bookUri in allBookUris)
                {

                    Console.Write(counter);
                    counter++;
                    if (db.Books.Where(b => b.Uri == bookUri).Count() > 0)
                    {
                        continue;
                    }
                    db.Books.Add(new Book
                    {
                        Uri = bookUri
                    });

                }
                db.SaveChanges();
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Books updated");


            // System.Console.WriteLine("Books inserted into database");

            // using (var db = new BookRecommenderContext())
            // {
            //     foreach (var book in db.Books)
            //     {
            //         GetBookNames(book);
            //     }
            // }



            // using (var db = new BookRecommenderContext())
            // {

            //     var names = GetBooksNames();

            //     System.Console.WriteLine("SPARQL executed");

            //     var counter = new Counter(names.Data.Count);
            //     System.Console.Write(counter);

            //     var sw = new Stopwatch();
            //     sw.Start();

            //     foreach (var item in names.Data)
            //     {
            //         var uri = item["item"];
            //         var book = db.Books.Where(b => b.Uri == uri)?.FirstOrDefault();
            //         if(book == null){
            //             System.Console.WriteLine("book not in database: " + uri);
            //             continue;
            //         }
            //         book.Title = item["title"];
            //         book.NameCs = item["label_cs"];
            //         book.NameEn = item["label_en"];

            //         db.Books.Update(book);
            //         counter++;
            //         System.Console.Write(counter);
            //     }
            //     sw.Stop();
            //     System.Console.WriteLine();
            //     System.Console.WriteLine(sw.ElapsedMilliseconds);

            //     db.SaveChanges();
            // }

            // using (var db = new BookRecommenderContext())
            // {

            //     var result = GetBooksOrigName();
            //     var counter = new Counter(db.Books.Count());

            //     foreach (var item in result.Data)
            //     {
            //         System.Console.Write(counter);
            //         var uri = item["item"];
            //         var langCode = item["lang_code"];
            //         var label = item["label"];

            //         var book = db.Books.Where(b => b.Uri == uri)?.FirstOrDefault();
            //         if (book == null)
            //         {
            //             System.Console.WriteLine("book not in database: " + uri);
            //             continue;
            //         }
            //         book.OrigLang = langCode;
            //         book.NameOrig = label;
            //         db.Books.Update(book);

            //     }

            //     System.Console.Write(counter);

            //     db.SaveChanges();
            // }


            // try to get original label by exploring authors country of origin
            //     using (var db = new BookRecommenderContext())
            //     {

            //         var result = GetBooksNamesByAuthorCountryLang();
            //         var counter = new Counter(db.Books.Count());

            //         foreach (var item in result.Data)
            //         {
            //             System.Console.Write(counter);
            //             var uri = item["item"];
            //             var langCode = item["country_lang_code"];
            //             var label = item["label"];

            //             var book = db.Books.Where(b => b.Uri == uri)?.FirstOrDefault();
            //             if (book == null)
            //             {
            //                 System.Console.WriteLine("book not in database: " + uri);
            //                 continue;
            //             }
            //             if(!string.IsNullOrEmpty(book.OrigLang)){
            //                 // book already contain orig lang name
            //                 continue;
            //             }

            //             book.OrigLang = langCode;
            //             book.NameOrig = label;
            //             db.Books.Update(book);
            //         }

            //         System.Console.Write(counter);

            //         db.SaveChanges();
            //     }

        }

      

    }

}