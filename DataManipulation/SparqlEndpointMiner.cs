using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace BookRecommender.DataManipulation
{
    abstract class SparqlEndPointMiner
    {
        // Delegate used to have generic thing to do with data, when more data involved, then use valueTuples
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
                    // If something went wrong, wait 10 sec and then try again
                    numberOfTries++;

                    System.Console.WriteLine(ex.ToString());
                    System.Console.WriteLine("Try again, attempt number " + numberOfTries);
                }

            // If something went wrong, wait 10 sec and then try again
            if(!successfull){
                System.Threading.Tasks.Task.Delay(10000).Wait();
            }

            } while (!successfull);
        }
        protected abstract List<Dictionary<string, string>> Execute(string query);

        // User can chose between updating all or only some
        // Make logic on the methodList numbers what ever you want, but the preferred one is:
        // Have some preferred ordering of subquery calls and then expose this numbering to user\
        // that he can call whatever combination of methods he desires
        public abstract void UpdateBooks(List<int> methodList);
        public abstract void UpdateAuthors(List<int> methodList);
        public abstract void UpdateCharacters(List<int> methodList);
        public abstract void UpdateGenres(List<int> methodList);
    }
}