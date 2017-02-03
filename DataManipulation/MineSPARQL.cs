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
        public static void Mine(string[] param)
        {
            SparqlEndPointMiner endpoint = new WikiDataEndpointMiner();
            if (param.Length == 1)
            {
                // no param to mine -> mine all
                endpoint.UpdateBooks(null);
                endpoint.UpdateAuthors(null);
            }
            else
            {
                // longer than one
                var methodNumberList = new List<int>();
                for(var i = 2; i < param.Length; i++){
                    int result;
                    var isNumber = int.TryParse(param[i], out result);

                    if(!isNumber){
                        System.Console.WriteLine($"Invalid mine parametres: {param[i]}");
                        return;
                    }

                    methodNumberList.Add(result);
                }

                switch (param[1])
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
    }
}