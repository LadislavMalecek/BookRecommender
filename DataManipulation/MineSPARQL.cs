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

            // using(var db = new BookRecommenderContext()){

            //     var allBookUris = GetAllBooksUris().ToList();

            //     //Create new console progress counter
            //     var counter = new Counter(allBookUris.Count);

            //     //Insert all books in database
            //     foreach (var bookUri in allBookUris){
            //         db.Books.Add(new Book {
            //             Uri = bookUri
            //         });

            //         counter++;
            //         Console.Write(counter);
            //     }
            //     db.SaveChanges();
            // }

            // using (var db = new BookRecommenderContext())
            // {
            //     foreach (var book in db.Books)
            //     {
            //         GetBookNames(book);
            //     }
            // }

            GetBooksNames();
        }

        static string ExecQuery(string query)
        {
            var request = WebRequest.Create(
                $"https://query.wikidata.org/sparql?query={query}&format=json"
            );
            request.Method = "GET";
            var httpResponse = request.GetResponseAsync().Result;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
        static IEnumerable<string> GetAllBooksUris()
        {
            var query = @"SELECT ?item
                        WHERE {
                            ?item wdt:P31 wd:Q571.
                        }";
            var Result = ExecQuery(query);
            var jObject = JObject.Parse(Result);
            var bookUris = from book in jObject["results"]["bindings"]
                           select (string)book["item"]["value"];
            return bookUris;
        }

        // If using verbatim and interpolation string then we need to double curl brackets and double quotes if we need to use them in string
        static void GetBookNames(Book book)
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


            //System.Console.WriteLine(query);

            var result = ExecQuery(query);

            System.Console.WriteLine("After exec");


            var data = JsonSparqlParser.JsonToList(result);

            System.Console.WriteLine(data);
            
            System.Console.WriteLine();
            System.Console.WriteLine();

            // System.Console.WriteLine("After Json parse");

            // var title =  GetFromJsonFirst(jObject, "title");
            // System.Console.WriteLine(title);

            // var labelEn = GetFromJsonFirst(jObject, "label_en");
            // System.Console.WriteLine(labelEn);

            // var labelCs = GetFromJsonFirst(jObject, "label_cs");
            // System.Console.WriteLine(labelCs);

            // System.Console.WriteLine("---------");

        }

        static void GetBooksNames()
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
                        }} LIMIT 500";



            var result = ExecQuery(query);

            var data = JsonSparqlParser.JsonToList(result);

            System.Console.WriteLine(data);
            

        }

        public static IEnumerable<string> GetFromJson(JObject jObject, string whatToRetrive)
        {
            try
            {
                return from obj in jObject["results"]["bindings"]
                       select (string)obj[whatToRetrive]["value"];
            }
            catch (NullReferenceException ex)
            {
                System.Console.WriteLine(ex);
                return null;
            }
        }
        public static string GetFromJsonFirst(JObject jObject, string whatToRetrive)
        {
            try
            {
                return GetFromJson(jObject, whatToRetrive).FirstOrDefault();
            }
            catch (NullReferenceException ex)
            {
                System.Console.WriteLine(ex);
                return null;
            }
        }
    }
    
}