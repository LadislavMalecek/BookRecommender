using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace BookRecommender.DataManipulation
{
    /// <summary>
    /// This is an abstract class which you need to inherit from in order to add a new data point.
    /// We will describe in a details steps that are necessary to be made.
    /// Please use the already working WikiDataEndpoint 
    /// and its components as a template for creating a new datapoint.
    /// First, read the methods descriptions.
    /// Second, try to implement it the best you can.
    /// Third, this is a small project so forget about searching on StackOverflow.
    /// And if you still don't know, JUST IMPLEMENT a child of this CLASS
    /// Even after all this, you had no luck? Contact me on my email which is malecek.ladislav at gmail.com
    /// Ill be happy to help, no seriously, I have worked on this project for 6 months, I would be flattered if someone
    /// wants to continue in my work.
    /// </summary>
    abstract class SparqlEndPointMiner
    {
        
        /// <summary>
        /// Delegate used to have generic thing to do with data, when more data involved, then use valueTuples
        /// All mining goes through this delegate, first you need to specify how to extract the data, and then specify the action for
        /// every single line of the extracted data using this line action.
        /// </summary>
        /// <param name="line">Line to proccess</param>
        /// <param name="db">What to do with the line</param>
        protected delegate void LineAction<T>(T line, BookRecommenderContext db);

        /// <summary>
        /// Method which takes IEnumerable with the data and calls the assigned line action on every line of the data.
        /// The data needs to fit inside the line action.
        /// This method is meant to be used when mining from command line
        /// </summary>
        /// <param name="data">Enumerable with the data lines</param>
        /// <param name="lineAction">Action which will be executed on each line</param>
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
                            // Insert all books in database
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

        /// <summary>
        /// Method which takes IEnumerable with the data and calls the assigned line action on every line of the data.
        /// The data needs to fit inside the line action.
        /// This method is meant to be used when mining from web interface
        /// </summary>
        /// <param name="data">Enumerable with the data lines</param>
        /// <param name="lineAction">Action which will be executed on each line</param>
        /// <param name="miningState">Mining state of the operation from the MiningProxySingleton used to monitor the data mining</param>
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
                // set the mining state
                miningState.CurrentState = MiningStateType.RunningQueryingEndpoint;
                var listData = data.ToList();
                miningState.CurrentState = MiningStateType.Running;
                var currentPosition = 0;
                using (var db = new BookRecommenderContext())
                {

                    // proccess each line using the line action and log changes to the mining state
                    foreach (var line in listData)
                    {
                        lineAction(line, db);
                        currentPosition++;
                        miningState.Message = String.Format("{0}/{1}",
                                 currentPosition, listData.Count);
                    }
                    miningState.CurrentState = MiningStateType.RunningSavingToDatabase;
                    db.SaveChanges();
                    miningState.CurrentState = MiningStateType.Completed;
                    miningState.Message = DateTime.Now.ToString();
                }
            }
            catch (Exception ex)
            {
                // If something went wrong, wait 10 sec and then try again
                miningState.CurrentState = MiningStateType.Error;
                miningState.Message = ex.Message;
            }
        }

        /// <summary>
        /// Generates id from uri according to unique 2letters and underscore
        /// It is used when saving tags to a file system, so please do not include any system forbidden characters
        /// Example for wikidata: http://www.wikidata.org/entity/Q442 => WD_Q422
        /// </summary>
        /// <param name="uri">URI identifier</param>
        /// <returns></returns>
        public abstract string GetIdFromUri(string uri);

        /// <summary>
        /// Generates uri from id according to unique 2letters and underscore
        /// It should be bijection with the GetIdFromUri being inverse function
        /// Example for wikidata: WD_Q422 => http://www.wikidata.org/entity/Q442
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract string GetUriFromId(string id);
        protected abstract List<Dictionary<string, string>> Execute(string query);

        // User can chose between updating all or only some
        // Make logic on the methodList numbers what ever you want, but the preferred one is:
        // Have some preferred ordering of subquery calls and then expose this numbering to user\
        // that he can call whatever combination of methods he desires

        /// <summary>
        /// Method which call the right updating the database with correct action and data pairs
        /// </summary>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
        public abstract void UpdateBooks(List<int> methodList, MiningState miningState = null);
        public void UpdateBooks(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateBooks(list, miningState);
        }
        /// <summary>
        /// Method which call the right updating the database with correct action and data pairs
        /// </summary>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
        public abstract void UpdateAuthors(List<int> methodList, MiningState miningState = null);
        public void UpdateAuthors(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateAuthors(list, miningState);
        }
        /// <summary>
        /// Method which call the right updating the database with correct action and data pairs
        /// </summary>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
        public abstract void UpdateCharacters(List<int> methodList, MiningState miningState = null);
        public void UpdateCharacters(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateCharacters(list, miningState);
        }
        /// <summary>
        /// Method which call the right updating the database with correct action and data pairs
        /// </summary>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
        public abstract void UpdateGenres(List<int> methodList, MiningState miningState = null);
        public void UpdateGenres(int methodNumber, MiningState miningState = null)
        {
            var list = new List<int>() { methodNumber };
            UpdateGenres(list, miningState);
        }

        /// <summary>
        /// This method calls right update method based on the mining entity type
        /// </summary>
        /// <param name="type">Type of the mining entity</param>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
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
                    UpdateGenres(methodList, miningState);
                    break;
                case MiningEntityType.Characters:
                    UpdateCharacters(methodList, miningState);
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }

        /// <summary>
        /// This method forward the call to the Update method with adding the single method number inside a single item int list
        /// </summary>
        /// <param name="type">Type of the mining entity</param>
        /// <param name="methodList">List of methods to be executed</param>
        /// <param name="miningState">Mining state to monitor the progress of the mining</param>
        public void Update(MiningEntityType type, int methodNumber, MiningState miningState)
        {
            var list = new List<int>() { methodNumber };
            Update(type, list, miningState);
        }

        /// <summary>
        /// Mining the dynamic additional data.
        /// </summary>
        /// <param name="entityUrl">URL of the entity for which we want to retrieve the data</param>
        /// <returns></returns>
        public abstract AdditionalSparqlData GetAdditionalData(string entityUrl);

        /// <summary>
        /// Method to retrieve the wikipedia pages
        /// It may be moved elsewhere. If implementing new endpoint just return empty list.
        /// </summary>
        /// <returns>Wikipedia pages information</param>
        public abstract IEnumerable<(string bookId, string wikiPageUrl)> GetBooksWikiPages();

        /// <summary>
        /// Get the unique name of the endpoint, used inside web mining interface
        /// </summary>
        /// <returns>Unique endpoint name</returns>
        public abstract string GetName();
    }
}