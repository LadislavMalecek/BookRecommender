using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace BookRecommender.DataManipulation
{
    /// <summary>
    /// It handles requests for images which are not available from the LOD data
    /// We are using image extraction from the Google search image web page. Google also offers,
    /// paid service to search for images, so there is risk of this service not beeing available.
    /// If Google makes assumption that our traffic is machine generated.
    /// </summary>
    class GoogleImageMiner
    {
        /// <summary>
        /// This method tries to get URI to the first google image search item of corresponding query
        /// </summary>
        /// <param name="queryItems">query to be send to Google Image search</param>
        /// <returns>Url of the first image on the Google search page</returns>
        public async Task<string> GetFirstImageUrlAsync(string queryItems)
        {
            try
            {
                string html = await GetHtmlCodeAsync(queryItems.Split(new char[] { ' ' }));
                //File.WriteAllText("html.txt", html);
                return GetFirstHttpsUrl(html);
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Retrieves HTML page from url of Google Image search using simple HTTP client
        /// </summary>
        /// <param name="queryItems">search params</param>
        /// <returns>HTML page file</returns>
        async Task<string> GetHtmlCodeAsync(string[] queryItems)
        {
            var queryUrl = "https://www.google.cz/search?q=" + string.Join("+", queryItems) + "&tbm=isch";

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(queryUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html; charset=UTF-8");
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.98 Safari/537.36");

            try
            {
                var task = httpClient.GetAsync(queryUrl);
                var result = await Task.WhenAny(task, Task.Delay(3000));
                if (result != task)
                {
                    // Timeout
                    return null;
                }
                var response = task.Result;
                var content=  await response.Content.ReadAsStringAsync();

                //File.WriteAllText("C:\\netcore\\stranka.html", content);
                
                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }
        string Deescape(string s)
        {
            Regex rx = new Regex(@"\\[uU]([0-9A-Fa-f]{4})");
            var result = rx.Replace(s, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            return result;
        }
        string GetFirstUrl(string html)
        {
            var urls = new List<string>();

            int start = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            if (start < 0)
            {
                return null;
            }
            start = html.IndexOf("\"", start + 4, StringComparison.Ordinal) + 1;
            int end = html.IndexOf("\"", start, StringComparison.Ordinal);
            string url = html.Substring(start, end - start);
            return Deescape(url);
        }
        string GetFirstHttpsUrlSLOW(string html){
            var list = GetUrls(html);
            var firstHttps = list.FirstOrDefault(url => url.ToLower().StartsWith("https://"));
            return firstHttps;
        }


        List<string> GetUrls(string html)
        {
            var urls = new List<string>();

            int start = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (start >= 0)
            {
                start = html.IndexOf("\"", start + 4, StringComparison.Ordinal);
                start++;
                int end = html.IndexOf("\"", start, StringComparison.Ordinal);
                string url = html.Substring(start, end - start);
                urls.Add(url);
                start = html.IndexOf("\"ou\"", end, StringComparison.Ordinal);
            }
            return urls;
        }

        string GetFirstHttpsUrl(string html)
        {
            int start = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (start >= 0)
            {
                start = html.IndexOf("\"", start + 4, StringComparison.Ordinal);
                start++;
                int end = html.IndexOf("\"", start, StringComparison.Ordinal);
                string url = html.Substring(start, end - start);
                if(url.StartsWith("https://")){
                    return url;
                }
                start = html.IndexOf("\"ou\"", end, StringComparison.Ordinal);
            }
            return null;
        }
    }
}