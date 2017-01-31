using System;
using System.Collections.Generic;
using System.Text;

namespace BookRecommender.DataManipulation
{
    public class SparqlData
    {
        // Stores the names of Query variables
        public List<string> Variables = new List<string>();

        // Stores data as a table, where rows corresponds to single Sparql result line
        // and column stores data according to values described in Variables
        public List<Dictionary<string,string>> Data = new List<Dictionary<string,string>>();

        // public IEnumerator<Dictionary<string, string>> GetEnumerator(){
        //     var dictionary = new Dictionary<string, string> 
        //     foreach(var row in Data){

        //     }
        // }

        public bool HasVariable(string name){
            return Variables.Contains(name);
        }

        public void InsertLine(List<string> line){
            if(line.Count != Variables.Count){
                throw new ArgumentException("Line is not the same size as Variables");
            }

            var retDictionary = new Dictionary<string,string>();
            for (int i = 0; i < line.Count; i++)
            {
                retDictionary.Add(Variables[i],line[i]);
            }
            Data.Add(retDictionary);
        }

        IEnumerable<string> GetColumn(string name){
            foreach(var row in Data){
                yield return row[name];
            }
        }

        IEnumerable<Dictionary<string, string>> GetColumns(params string[] names){
            foreach(var row in Data){
                var returnDictionary = new Dictionary<string, string>();
                foreach(var name in names){
                    returnDictionary.Add(name, row[name]);
                }
                yield return returnDictionary;
            }
        }


        // indexer to return lazy enumerable of one data column
        public IEnumerable<string> this[string s]{
            get{
                return GetColumn(s);
            }
        }

        public IEnumerable<Dictionary<string, string>> this[params string[] s]{
            get{
                return GetColumns(s);
            }
        }

        public override string ToString()
        {
            var sB = new StringBuilder();
            foreach (var variable in Variables)
            {
                sB.Append(variable + " ,");
            }
            sB.Append(Environment.NewLine);
            sB.Append("----------------");
            sB.Append(Environment.NewLine);
            foreach (var data in Data)
            {
                foreach (var item in data)
                {
                    sB.Append(item + " ,");
                }
                sB.Append(Environment.NewLine);
            }
            return sB.ToString();
        }

    }

}