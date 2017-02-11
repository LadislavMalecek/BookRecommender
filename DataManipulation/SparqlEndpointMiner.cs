using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace BookRecommender.DataManipulation
{
    abstract class SparqlEndPointMiner
    {
        protected delegate void LineAction<T>(T line, BookRecommenderContext db);
        protected void UpdateDatabase<T>(IEnumerable<T> data, LineAction<T> lineAction)
        {
            // Writeout the delegate name
            System.Console.WriteLine(lineAction.GetMethodInfo().Name);

            bool successfull = false;
            do
            {
                var numberOfTries = 1;
                try
                {
                    //Execute query -- retrieve collection only once
                    System.Console.WriteLine("Querying Endpoind");
                    var listData = data.ToList();
                    System.Console.WriteLine("Updating database");
                    using (var db = new BookRecommenderContext())
                    {
                        //Create new console progress counter
                        using (var counter = new Counter(listData.Count))
                        {
                            //Insert all books in database
                            foreach (var line in listData)
                            {
                                lineAction(line, db);
                                counter.Update();
                            }
                        }
                        System.Console.WriteLine("Saving database");
                        db.SaveChanges();
                        successfull = true;
                    }
                }
                catch (Exception ex)
                {
                    numberOfTries++;

                    System.Console.WriteLine(ex.ToString());
                    System.Console.WriteLine("Try again, attempt number " + numberOfTries);
                }
            } while (!successfull);
        }
        protected abstract List<Dictionary<string, string>> Execute(string query);
        public abstract void UpdateBooks(List<int> methodList);
        public abstract void UpdateAuthors(List<int> methodList);
        public abstract void UpdateCharacters(List<int> methodList);
        public abstract void UpdateGenres(List<int> methodList);
    }
}