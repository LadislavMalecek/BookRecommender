using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BookRecommender.Models;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation
{

    public class MineSPARQL
    {
        public static void Mine()
        {

            using(var db = new BookRecommenderContext()){

                var allBookUris = GetAllBooksUris().ToList();

                //Create new console progress counter
                var counter = new Counter(allBookUris.Count);

                //Insert all books in database
                foreach (var bookUri in allBookUris){
                    Console.Write(counter);
                    counter++;
                    if(db.Books.Where(b =>b.Uri == bookUri).Count() > 0){
                        continue;
                    }
                    db.Books.Add(new Book {
                        Uri = bookUri
                    });

                }
                db.SaveChanges();
            }

            // using (var db = new BookRecommenderContext())
            // {
            //     foreach (var book in db.Books)
            //     {
            //         GetBookNames(book);
            //     }
            // }

            using (var db = new BookRecommenderContext())
            {

                var names = GetBooksNames();

                System.Console.WriteLine("SPARQL executed");

                var counter = new Counter(names.Data.Count);
                System.Console.Write(counter);
                

                foreach (var item in names.Data)
                {
                    var uri = item["item"];
                    var book = db.Books.Where(b => b.Uri == uri)?.First();
                    if(book == null){
                        System.Console.WriteLine("book not in database: " + uri);
                    }
                    book.Title = item["title"];
                    book.NameCs = item["label_cs"];
                    book.NameEn = item["label_en"];
                    
                    db.Books.Update(book);
                    counter++;
                    System.Console.Write(counter);
                }
                db.SaveChanges();
            }

        }


        static IEnumerable<string> GetAllBooksUris()
        {
            var query = @"SELECT ?item
                        WHERE {
                            ?item wdt:P31 wd:Q571.
                        }";
            var result = new WikiDataMiner().MineData(query);

            return result["item"];
        }

        // If using verbatim and interpolation string then we need to double curl brackets and double quotes if we need to use them in string
        static SparqlResult GetBookNames(Book book)
        {

            System.Console.WriteLine(book.Uri);
            var query = $@"SELECT ?title ?label_cs ?label_en
                        WHERE {{
                            ?item wdt:P31 wd:Q571.
                                FILTER(?item = <{book.Uri}>)
                            OPTIONAL{{
                                ?item wdt:P1476 ?title.
                            }}
                            OPTIONAL{{
                                ?item rdfs:label ?label_cs.
                                    FILTER(LANG(?label_cs) = ""cs"")
                            }}
                            OPTIONAL{{
                                ?item rdfs:label ?label_en.
  	                                FILTER(LANG(?label_en) = ""en"")
                            }}
                        }} LIMIT 1";

            return new WikiDataMiner().MineData(query);
        }

        static SparqlResult GetBooksNames()
        {

            var query = $@"SELECT ?item ?title ?label_cs ?label_en
                        WHERE {{
                            ?item wdt:P31 wd:Q571.
                            OPTIONAL{{
                                ?item wdt:P1476 ?title.
                            }}
                            OPTIONAL{{
                                ?item rdfs:label ?label_cs.
                                    FILTER(LANG(?label_cs) = ""cs"")
                            }}
                            OPTIONAL{{
                                ?item rdfs:label ?label_en.
  	                                FILTER(LANG(?label_en) = ""en"")
                            }}
                        }}";

            return new WikiDataMiner().MineData(query);
        }

        // public static IEnumerable<string> GetFromJson(JObject jObject, string whatToRetrive)
        // {
        //     try
        //     {
        //         return from obj in jObject["results"]["bindings"]
        //                select (string)obj[whatToRetrive]["value"];
        //     }
        //     catch (NullReferenceException ex)
        //     {
        //         System.Console.WriteLine(ex);
        //         return null;
        //     }
        // }
        // public static string GetFromJsonFirst(JObject jObject, string whatToRetrive)
        // {
        //     try
        //     {
        //         return GetFromJson(jObject, whatToRetrive).FirstOrDefault();
        //     }
        //     catch (NullReferenceException ex)
        //     {
        //         System.Console.WriteLine(ex);
        //         return null;
        //     }
        // }
    }

}