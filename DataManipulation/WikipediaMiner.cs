using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookRecommender.DataManipulation
{
    class WikipediaMiner
    {
        const string UrlAppendix = "?action=raw";
        public async Task<string> Mine(string pageUrl)
        {
            var page = await Exec(pageUrl + UrlAppendix);

            page = RemoveCurlyBraces(page);
            page = RemoveSingleReferences(page);
            page = RemoveReferences(page);
            page = RemoveComments(page);
            page = KeepOnlyText(page);
            return page;
        }
        async Task<string> Exec(string url)
        {
            var request = HttpWebRequest.Create(url);
            request.Method = "GET";
            try
            {
                var task = request.GetResponseAsync();
                if (Task.WhenAny(task, Task.Delay(3000)).Result != task)
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
            catch (Exception)
            {
                return null;
            }
        }

        string KeepOnlyText(string text)
        {
            var sb = new StringBuilder();
            bool lastWasText = false;
            foreach (var c in text)
            {
                if (char.IsLetter(c))
                {
                    sb.Append(c);
                    lastWasText = true;
                }
                else
                {
                    if (lastWasText)
                    {
                        sb.Append(' ');
                    }
                    lastWasText = false;
                }
            }
            return sb.ToString();
        }
        string RemoveSingleReferences(string text)
        {
            var sBuilder = new StringBuilder();

            bool inElement = false;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '<' && text[i + 1] == 'r' && text[i + 2] == 'e' && text[i + 3] == 'f' && text[i + 4] == ' ')
                {
                    inElement = true;
                    i += 4;
                    continue;
                }
                if (text[i] == '/' && text[i + 1] == '>')
                {
                    inElement = false;
                    //skip next char
                    i += 1;
                    continue;
                }
                if (!inElement)
                {
                    sBuilder.Append(text[i]);
                }
            }
            return sBuilder.ToString();
        }

        string RemoveReferences(string text)
        {
            var sBuilder = new StringBuilder();

            int levelsIn = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<' && text[i + 1] == 'r' && text[i + 2] == 'e' && text[i + 3] == 'f' && text[i + 4] == '>')
                {
                    levelsIn++;
                    //skip next char
                    i += 4;
                    continue;
                }
                if (text[i] == '<' && text[i + 1] == '/' && text[i + 2] == 'r' && text[i + 3] == 'e' && text[i + 4] == 'f' && text[i + 5] == '>')
                {
                    levelsIn--;
                    //skip next char
                    i += 5;
                    continue;
                }
                if (levelsIn == 0)
                {
                    sBuilder.Append(text[i]);
                }
                if (levelsIn < 0)
                {
                    throw new NotSupportedException();
                }
            }
            return sBuilder.ToString();
        }
        string RemoveComments(string text)
        {
            var sBuilder = new StringBuilder();

            int levelsIn = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<' && text[i + 1] == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                {
                    levelsIn++;
                    //skip next char
                    i += 3;
                    continue;
                }
                if (text[i] == '-' && text[i + 1] == '-' && text[i + 2] == '>')
                {
                    levelsIn--;
                    //skip next char
                    i += 2;
                    continue;
                }
                if (levelsIn == 0)
                {
                    sBuilder.Append(text[i]);
                }
                if (levelsIn < 0)
                {
                    throw new NotSupportedException();
                }
            }
            return sBuilder.ToString();
        }

        string RemoveCurlyBraces(string text)
        {
            var sBuilder = new StringBuilder();

            int levelsIn = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{' && text[i + 1] == '{')
                {
                    levelsIn++;
                    //skip next char
                    i++;
                    continue;
                }
                if (text[i] == '}' && text[i + 1] == '}')
                {
                    levelsIn--;
                    //skip next char
                    i++;
                    continue;
                }
                if (levelsIn == 0)
                {
                    sBuilder.Append(text[i]);
                }
                if (levelsIn < 0)
                {
                    throw new NotSupportedException();
                }
            }
            return sBuilder.ToString();
        }

        //-------------------------- SLOW  - deprecated ---------------------------------
        //-------------------------------------------------------------------------------
        // static string RemoveNested(string text, string left, string right)
        // {
        //     var sBuilder = new StringBuilder();

        //     int levelsIn = 0;
        //     for (var i = 0; i < text.Length; i++)
        //     {
        //         var start = Math.Max(0, sBuilder.Length - 50);
        //         var s = sBuilder.ToString().Substring(start);

        //         if (i <= text.Length - left.Length && text.Substring(i, left.Length) == left)
        //         {
        //             levelsIn++;
        //             //skip next char
        //             i += left.Length - 1;
        //             continue;
        //         }
        //         if (i <= text.Length - right.Length && text.Substring(i, right.Length) == right)
        //         {
        //             levelsIn--;
        //             //skip next char
        //             i += right.Length - 1;
        //             continue;
        //         }
        //         if (levelsIn == 0)
        //         {
        //             sBuilder.Append(text[i]);
        //         }
        //         if (levelsIn < 0)
        //         {
        //             throw new NotSupportedException();
        //         }
        //     }
        //     return sBuilder.ToString();
        // }
        // static string RemoveSingle(string text, string left, string right)
        // {
        //     var sBuilder = new StringBuilder();

        //     bool inElement = false;
        //     for (var i = 0; i < text.Length; i++)
        //     {
        //         if (i <= text.Length - left.Length && text.Substring(i, left.Length) == left)
        //         {
        //             inElement = true;
        //             i += left.Length - 1;
        //             continue;
        //         }
        //         if (i <= text.Length - right.Length && text.Substring(i, right.Length) == right)
        //         {
        //             inElement = false;
        //             //skip next char
        //             i += right.Length - 1;
        //             continue;
        //         }
        //         if (!inElement)
        //         {
        //             sBuilder.Append(text[i]);
        //         }
        //     }
        //     return sBuilder.ToString();
        // }
    }
}