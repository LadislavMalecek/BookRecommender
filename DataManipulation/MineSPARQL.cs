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

namespace BookRecommender.DataManipulation
{

    public class MineSPARQL
    {
        public static void Mine(string[] args)
        {
            SparqlEndPointMiner endpoint = new WikiDataEndpointMiner();
            if (args.Length == 1)
            {
                // no param to mine -> mine all
                endpoint.UpdateBooks(null);
                endpoint.UpdateAuthors(null);
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

                switch (args[1])
                {
                    case "books":
                        endpoint.UpdateBooks(methodNumberList);
                        break;
                    case "authors":
                        endpoint.UpdateAuthors(methodNumberList);
                        break;
                    default:
                        System.Console.WriteLine("Param not supported");
                        break;
                }
            }
        }
        static void Test()
        {
            new CsvParserTests().SingleMultiLine();
        }
    }
}