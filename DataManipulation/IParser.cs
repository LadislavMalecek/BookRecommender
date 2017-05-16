using System.Collections.Generic;

namespace BookRecommender.DataManipulation{

    /// <summary>
    /// Interface to be used with custom instances of SPARQL data parser.
    /// </summary>
    interface IParser
    {
        List<Dictionary<string,string>> Parse(string data);
    }
}