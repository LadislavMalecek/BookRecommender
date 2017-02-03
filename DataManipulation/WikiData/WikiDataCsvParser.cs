using System;
using System.Collections.Generic;
using System.Text;
using BookRecommender.DataManipulation;
using System.IO;

namespace BookRecommender.DataManipulation.WikiData
{
    class WikiDataCsvParser : IParser
    {
        public List<Dictionary<string, string>> Parse(string data)
        {
            var list = new CsvParser(data).ParseToList();

            if(list.Count == 0){
                return null;
            }

            var variables = list[0];

            var retList = new List<Dictionary<string, string>>();

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
                throw new InvalidDataException(errorSB.ToString());
            }
            return retList;
        }
    }
}