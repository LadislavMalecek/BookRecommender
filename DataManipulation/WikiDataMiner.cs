using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.IO;

namespace BookRecommender.DataManipulation{


    class WikiDataMiner : IMiner
    {
        public SparqlResult MineData(string query){
                var queryResult = ExecQuery(query);
                var normalizedData = JsonToSparqlResult(queryResult);
                return normalizedData;
        }

        string ExecQuery(string query)
        {
            var request = WebRequest.Create(
                $"https://query.wikidata.org/sparql?query={query}&format=json"
            );
            request.Method = "GET";
            var httpResponse = (HttpWebResponse)request.GetResponseAsync().Result;
            
            //var statusCode = httpResponse.StatusCode;
            
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
        SparqlResult JsonToSparqlResult(string json)
        {
            var jObject = JObject.Parse(json);

            var head = jObject?["head"]?["vars"]?.Children();

            var returnData = new SparqlResult();

            //Load variables from json to list
            foreach (var variable in head)
            {
                returnData.Variables.Add((string)variable);
            }

            //If there is no variable in json, there is also no data
            if (returnData.Variables.Count == 0)
            {
                return null;
            }

            var objects = jObject?["results"]?["bindings"];
            foreach (var obj in objects)
            {

                var objectList = new Dictionary<string,string>();

                //try to retrive data for every variable, else get empty string
                foreach (var variable in returnData.Variables)
                {
                    var value = (string)obj?[variable]?["value"];
                    if (value == null)
                    {
                        objectList.Add(variable, string.Empty);
                    }
                    else
                    {
                        objectList.Add(variable, value);
                    }
                }
                returnData.Data.Add(objectList);
            }
            return returnData;
        }
    }
}