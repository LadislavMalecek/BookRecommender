
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BookRecommender.DataManipulation.WikiPedia
{
    class WikiPageDownloader
    {
        const string UrlAppendix = "?&action=raw";
        public async Task<string> DownloadPage(string url)
        {
            var request = HttpWebRequest.Create(url +UrlAppendix);
            request.Method = "GET";
            try
            {
                var task = request.GetResponseAsync();
                if (Task.WhenAny(task, Task.Delay(30000)).Result != task)
                {
                    // Timeout
                    return null;
                }

                var httpResponse = (HttpWebResponse)await task;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error while downloading page: " + ex);
                return null;
            }
        }
    }
}