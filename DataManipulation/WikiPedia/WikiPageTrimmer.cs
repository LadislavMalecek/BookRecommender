using System;
using System.Text;

namespace BookRecommender.DataManipulation.WikiPedia
{
    class WikiPageTrimmer
    {
        public string Trim(string wikiPage)
        {
                var page = RemoveComments(wikiPage);
                page = RemoveCurlyBraces(page);
                page = RemoveReferences(page);
                page = KeepOnlyText(page);
                return page;
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
        string RemoveReferences(string text)
        {
            var sBuilder = new StringBuilder();

            bool inElement = false;
            int levelsIn = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (inElement)
                {
                    if (text[i] == '/' && text[i + 1] == '>')
                    {
                        inElement = false;
                        i++;
                        continue;
                    }
                    if (text[i] == '>')
                    {
                        inElement = false;
                        levelsIn++;
                        continue;
                    }
                    continue;
                }
                if (text[i] == '<' &&
                    (text[i + 1] == 'r' || text[i + 1] == 'R') &&
                    (text[i + 2] == 'e' || text[i + 2] == 'E') &&
                    (text[i + 3] == 'f' || text[i + 3] == 'F'))
                {
                    inElement = true;
                    //skip next char
                    i += 3;
                    continue;
                }
                if (text[i] == '<' &&
                    text[i + 1] == '/' &&
                    (text[i + 2] == 'r' || text[i + 2] == 'R') &&
                    (text[i + 3] == 'e' || text[i + 3] == 'E') &&
                    (text[i + 4] == 'f' || text[i + 4] == 'F') &&
                    text[i + 5] == '>')
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
                    var x = Math.Max(0, i - 50);
                    var y = Math.Min(50, text.Length - i);
                    var beforeExc = text.Substring(x, 50);
                    var afterExc = text.Substring(i, y);
                    throw new NotSupportedException("Level below zero TextBefore:" + beforeExc + "TextAfter:" + afterExc);
                }
            }
            return sBuilder.ToString();
        }
        string RemoveComments(string text)
        {
            var sBuilder = new StringBuilder();

            int levelsIn = 0;
            int charsAddedAfterLastRight = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var stringBuilder = sBuilder.ToString();
                var curChar = text[i];
                var cm = text.Substring(i);
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
                    if (levelsIn < 0)
                    {
                        levelsIn = 0;
                        sBuilder.Remove(sBuilder.Length - charsAddedAfterLastRight, charsAddedAfterLastRight);
                    }

                    charsAddedAfterLastRight = 0;
                    //skip next char
                    i += 2;
                    continue;
                }
                if (levelsIn == 0)
                {
                    charsAddedAfterLastRight++;
                    sBuilder.Append(text[i]);
                }
                if (levelsIn < 0)
                {
                    var x = Math.Max(0, i - 50);
                    var y = Math.Min(50, text.Length - i);
                    var beforeExc = text.Substring(x, 50);
                    var afterExc = text.Substring(i, y);
                    throw new NotSupportedException("Level below zero TextBefore:" + beforeExc + "TextAfter:" + afterExc);
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
                    var x = Math.Min(0, i - 50);
                    var beforeExc = text.Substring(x, 50);
                    throw new NotSupportedException("Level below zero TextBefore:" + beforeExc);
                }
            }
            return sBuilder.ToString();
        }
    }
}