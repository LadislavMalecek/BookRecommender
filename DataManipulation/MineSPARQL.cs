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
        public static void Mine(string param)
        {
            SparqlEndPointMiner endpoint = new WikiDataEndpointMiner();

            switch (param)
            {
                case "books":
                    endpoint.UpdateBooks();
                    break;
                case "authors":
                    endpoint.UpdateAuthors();
                    break;
                default:
                    System.Console.WriteLine("Param not supported");
                    break;
            }

        }
    }
}