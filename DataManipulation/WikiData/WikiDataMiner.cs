using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace BookRecommender.DataManipulation.WikiData
{
    class WikiDataMiner : IMiner
    {
        public enum FormatType
        {
            Json, Csv
        }
        FormatType formatType;
        public WikiDataMiner(FormatType formatType = FormatType.Csv)
        {
            this.formatType = formatType;
        }
        public string MineData(string query)
        {
            if (formatType == FormatType.Csv)
            {
                return ExecQueryCsv(query);
            }
            else
            {
                return ExecQueryJson(query);
            }
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
        string ExecQueryJson(string query)
        {
            var request = HttpWebRequest.Create(
                $"https://query.wikidata.org/sparql?query={query}&format=json"
            );
            request.Method = "GET";
            return Exec(request);
        }
        string Exec(WebRequest request)
        {
            HttpWebResponse httpResponse;
            try
            {
                var task = request.GetResponseAsync();
                if (Task.WhenAny(task, Task.Delay(30000)).Result != task)
                {
                    // Timeout
                    return null;
                }

                httpResponse = (HttpWebResponse)task.Result;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}