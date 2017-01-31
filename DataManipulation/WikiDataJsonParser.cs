using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BookRecommender.DataManipulation{
    class WikiDataJsonParser
    {
        public SparqlData Parse(string data)
        {
            var jObject = JObject.Parse(data);

            var head = jObject?["head"]?["vars"]?.Children();

            var returnData = new SparqlData();

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