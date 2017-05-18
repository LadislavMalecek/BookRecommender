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
    /// <summary>
    /// Simple http web request downloader
    /// implementing IDownloader specific to query Wikidata endpoint
    /// Specification of this API can be found in Wikidata documentation.
    /// </summary>
    class WikiDataDownloader : IDownloader
    {
        /// <summary>
        /// Supported return formats
        /// </summary>
        public enum FormatType
        {
            Json, Csv
        }
        FormatType formatType;
        public WikiDataDownloader(FormatType formatType = FormatType.Csv)
        {
            this.formatType = formatType;
        }
        /// <summary>
        /// Executes query on query.wikidata.org, that is spe
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string MineData(string query)
        {
            System.Console.WriteLine(query);
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
            System.Console.WriteLine(request.RequestUri.OriginalString);
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
                    var text =  streamReader.ReadToEnd();
                    return text;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                return null;
            }
        }
    }
}