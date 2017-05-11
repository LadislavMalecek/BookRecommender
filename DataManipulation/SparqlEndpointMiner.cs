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
                if (!successfull)
                {
                    System.Threading.Tasks.Task.Delay(10000).Wait();
                }

            } while (!successfull);
        }

        protected void UpdateDatabase<T>(IEnumerable<T> data, LineAction<T> lineAction, MiningState miningState)
        {
            // fall back for deprecated commandline mining
            if (miningState == null)
            {
                UpdateDatabase<T>(data, lineAction);
                return;
            }

            try
            {
                miningState.CurrentState = MiningStateType.RunningQueryingEndpoint;
                var listData = data.ToList();
                miningState.CurrentState = MiningStateType.Running;
                miningState.Count = listData.Count;
                using (var db = new BookRecommenderContext())
                {
                    //Insert all books in database
                    foreach (var line in listData)
                    {
                        lineAction(line, db);
                        miningState.CurrentPosition++;
                    }
                    miningState.CurrentState = MiningStateType.RunningSavingToDatabase;
                    db.SaveChanges();
                    miningState.CurrentState = MiningStateType.Completed;
                }
            }
            catch (Exception ex)
            {
                // If something went wrong, wait 10 sec and then try again
                miningState.CurrentState = MiningStateType.Error;
                miningState.Message = ex.Message;
            }
        }

        // Generates id from uri according to unique 2letters and underscore
        // Example for wikidata: http://www.wikidata.org/entity/Q442 => WD_Q422
        public abstract string GetIdFromUri(string uri);
        public abstract string GetUriFromId(string id);
        protected abstract List<Dictionary<string, string>> Execute(string query);

        // User can chose between updating all or only some
        // Make logic on the methodList numbers what ever you want, but the preferred one is:
        // Have some preferred ordering of subquery calls and then expose this numbering to user\
        // that he can call whatever combination of methods he desires
        public abstract void UpdateBooks(List<int> methodList, MiningState miningState = null);
        public void UpdateBooks(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateBooks(list, miningState);
        }
        public abstract void UpdateAuthors(List<int> methodList, MiningState miningState = null);
        public void UpdateAuthors(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateAuthors(list, miningState);
        }
        public abstract void UpdateCharacters(List<int> methodList, MiningState miningState = null);
        public void UpdateCharacters(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateCharacters(list, miningState);
        }
        public abstract void UpdateGenres(List<int> methodList, MiningState miningState = null);
        public void UpdateGenres(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateGenres(list, miningState);
        }

        public void Update(MiningEntityType type, List<int> methodList, MiningState miningState)
        {
            switch (type)
            {
                case MiningEntityType.Books:
                    UpdateBooks(methodList, miningState);
                    break;
                case MiningEntityType.Authors:
                    UpdateAuthors(methodList, miningState);
                    break;
                case MiningEntityType.Genres:
                    UpdateBooks(methodList, miningState);
                    break;
                case MiningEntityType.Characters:
                    UpdateCharacters(methodList, miningState);
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
        public void Update(MiningEntityType type, int methodNumber, MiningState miningState)
        {
            var list = new List<int>() { methodNumber };
            Update(type, list, miningState);
        }
        public abstract AdditionalSparqlData GetAdditionalData(string entityUrl);
        public abstract IEnumerable<(string bookId, string wikiPageUrl)> GetBooksWikiPages();

        public abstract string GetName();
    }
}