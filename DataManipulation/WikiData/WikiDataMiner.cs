using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.IO;

namespace BookRecommender.DataManipulation.WikiData
{
    class WikiDataMiner : IMiner
    {
        public string MineData(string query)
        {
            var queryResult = ExecQueryJson(query);
            return queryResult;
        }

        public SparqlData MineDataParse(string query)
        {
            string queryResult = ExecQueryCsv(query);

            var normalizedData = new CsvParser(queryResult).Parse();

            var retData = new SparqlData(){
                Variables = normalizedData[0]
            };

            foreach(var item in normalizedData.GetRange(1,normalizedData.Count -1)){
                retData.InsertLine(item);
            }

            return retData;
        }

        string ExecQueryJson(string query)
        {
            var request = HttpWebRequest.Create(
                $"https://query.wikidata.org/sparql?query={query}&format=json"
            );
            request.Method = "GET";
            var httpResponse = (HttpWebResponse)request.GetResponseAsync().Result;

            return Exec(request);
        }
        string ExecQueryCsv(string query)
        {
            var request = HttpWebRequest.Create(
                $"https://query.wikidata.org/sparql?query={query}"
            );
            request.Method = "GET";
            request.Headers["Accept"] = "text/csv";

            return Exec(request);
        }

        string Exec(WebRequest request)
        {
            var httpResponse = (HttpWebResponse)request.GetResponseAsync().Result;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}