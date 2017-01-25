using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System;
namespace BookRecommender.DataManipulation{


    class JsonSparqlParser
    {
        public class ParsedData
        {
            public List<string> Variables = new List<string>();
            public List<List<string>> Data = new List<List<string>>();

            public override string ToString(){
                var sB = new StringBuilder();
                foreach(var variable in Variables){
                    sB.Append(variable + " ,");
                }
                sB.Append(Environment.NewLine);
                sB.Append("----------------");
                sB.Append(Environment.NewLine);
                foreach(var data in Data){
                    foreach(var item in data){
                        sB.Append(item + " ,");
                    }
                    sB.Append(Environment.NewLine);
                }
                return sB.ToString(); 
            }

        }

        public static ParsedData JsonToList(string json)
        {
            var jObject = JObject.Parse(json);

            var head = jObject?["head"]?["vars"]?.Children();

            var returnData = new ParsedData();

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

                var objectList = new List<string>();

                //try to retrive data for every variable, else get empty string
                foreach (var variable in returnData.Variables)
                {
                    var value = (string)obj?[variable]?["value"];
                    if (value == null)
                    {
                        objectList.Add(string.Empty);
                    }
                    else
                    {
                        objectList.Add(value);
                    }
                }
                returnData.Data.Add(objectList);
            }
            return returnData;
        }
    }
}