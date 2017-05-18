using System;
using System.Collections.Generic;
using System.Text;
using BookRecommender.DataManipulation;
using System.IO;

namespace BookRecommender.DataManipulation.WikiData
{
    /// <summary>
    /// Simple wikidata  parser
    /// Implementing IParser for a specific WikiData use
    /// </summary>
    class WikiDataCsvParser : IParser
    {
        /// <summary>
        /// Parser that proccesses the returned data format. It is described at a Wikidata query API help page.
        /// </summary>
        /// <param name="data">Wikidata text data</param>
        /// <returns>parsed text, for each line there is dictionary with name and value</returns>
        public List<Dictionary<string, string>> Parse(string data)
        {
            var list = new CsvParser(data).ParseToList();


            var retList = new List<Dictionary<string, string>>();

            if(list.Count == 0){
                return retList;
            }

            var variables = list[0];

            var somethingWrong = false;
            var errorSB = new StringBuilder();

            for (int i = 1; i < list.Count; i++)
            {
                var dictionary = new Dictionary<string,string>();
                var line = list[i];

                if(line.Count != variables.Count)
                {
                    errorSB.Append(String.Join(", ", line));
                    errorSB.Append(Environment.NewLine);
                    somethingWrong = true;
                }
                for (int j = 0; j < line.Count; j++)
                {
                    dictionary.Add(variables[j], line[j]);
                }
                retList.Add(dictionary);
            }
            if(somethingWrong){
                System.Console.WriteLine(errorSB);
                throw new InvalidDataException(errorSB.ToString());
            }
            return retList;
        }
    }
}