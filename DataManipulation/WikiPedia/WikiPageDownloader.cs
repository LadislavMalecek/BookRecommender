
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BookRecommender.DataManipulation.WikiPedia
{
    /// <summary>
    /// Class which queries wikipedia to obtain only the text of the pages.
    /// Specification of this API can be found in documentation of Wikipedia.
    /// </summary>
    class WikiPageDownloader
    {
        const string UrlAppendix = "?&action=raw";

        /// <summary>
        /// Simple Http web request method that retrieves the specific page from wikipedia server.
        /// Be noted: when calling more than few tens of requests per second,
        /// the Wikipedia DOS protection will be triggered an you will be banned from issuing
        /// new requests for a while. 
        ///  </summary>
        /// <param name="url">URL of a page to retrieve</param>
        /// <returns>Raw wikipedia page</returns>
        public async Task<string> DownloadPageAsync(string url)
        {
            var request = HttpWebRequest.Create(url +UrlAppendix);
            request.Method = "GET";
            try
            {
                var task = request.GetResponseAsync();
                var taskOrDelay = await Task.WhenAny(task, Task.Delay(3000));

                if(taskOrDelay != task)
                {
                    // timeouted
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