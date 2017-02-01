using System.Collections.Generic;

namespace BookRecommender.DataManipulation{
    interface IParser
    {
        List<Dictionary<string,string>> Parse(string data);
    }
}