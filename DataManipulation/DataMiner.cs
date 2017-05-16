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
using BookRecommender.Tests;
using BookRecommender.DataManipulation.WikiPedia;
using System.Threading.Tasks;

namespace BookRecommender.DataManipulation
{
    /// <summary>
    /// This class is the start point of mining from console.
    /// This is deprecated, use the interface from the manage part of the webpage
    /// </summary>
    public class DataMiner
    {
        /// <summary>
        /// Deprecated, use the mining interface on the web.
        /// Starts mining operation based on the arguments provided
        /// </summary>
        /// <param name="args"></param>
        public static void Mine(string[] args)
        {
            // start all when no args present
            SparqlEndPointMiner endpoint = new WikiDataEndpointMiner();
            if (args.Length == 1)
            {
                // no param to mine -> mine all
                endpoint.UpdateBooks(null);
                endpoint.UpdateAuthors(null);
                endpoint.UpdateCharacters(null);
                endpoint.UpdateGenres(null);
            }
            else
            {
                if (args[1] == "test")
                {
                    Test();
                    return;
                }

                // longer than one
                var methodNumberList = new List<int>();
                for (var i = 2; i < args.Length; i++)
                {
                    int result;
                    var isNumber = int.TryParse(args[i], out result);

                    if (!isNumber)
                    {
                        System.Console.WriteLine($"Invalid mine parametres: {args[i]}");
                        return;
                    }

                    methodNumberList.Add(result);
                }

                switch (args[1].ToLower())
                {
                    case "books":
                        endpoint.UpdateBooks(methodNumberList);
                        break;
                    case "authors":
                        endpoint.UpdateAuthors(methodNumberList);
                        break;
                    case "characters":
                        endpoint.UpdateCharacters(methodNumberList);
                        break;
                    case "genres":
                        endpoint.UpdateGenres(methodNumberList);
                        break;
                    case "wikitags":
                        new WikiPageTagMiner().UpdateTags(methodNumberList);
                        break;

                    default:
                        System.Console.WriteLine("Param not supported");
                        break;
                }
            }
        }
        static void Test()
        {
            // var db = new BookRecommenderContext();
            // var wPages = db.Books.Select(b => b.WikipediaPage).Where(b => b != null);
            // foreach (var page in wPages)
            // {
            //     if(page ==null)
            //     {
            //         System.Console.WriteLine("wtf");
            //         return;
            //     }
            //     var file = BookRecommender.DataManipulation.WikiPedia.WikiPageStorage.GetFileNameFromUrl(page);
            //     if (file == null)
            //     {
            //         System.Console.WriteLine("'null: '" + page);
            //     }
            //     else
            //     {
            //         System.Console.WriteLine(file);
            //     }
            //     System.Console.ReadKey();
            // }
            // System.Console.WriteLine("Executing sparql query");
            // var sparqlData = new WikiDataEndpointMiner().GetBooksWikiPages().ToList();
            // System.Console.WriteLine("Downloading from wiki");
            // new WikiPageTagMiner().DownloadAndTrimPages(sparqlData);
        }
        /// <summary>
        /// Used as a middle man to mine additional data from WikiData query
        /// </summary>
        /// <param name="entityUrl">Url of entity</param>
        /// <returns>All available data about the entity</returns>
        public async static Task<AdditionalSparqlData> GetAdditionalDataAsync(string entityUrl)
        {
            // Branching by the entityUrl
            // I have only one service now, so gonna call WikidataMiner
            var task = new TaskFactory<AdditionalSparqlData>().StartNew(() => new WikiData.WikiDataEndpointMiner().GetAdditionalData(entityUrl));
            return await task;
        }
    }
}