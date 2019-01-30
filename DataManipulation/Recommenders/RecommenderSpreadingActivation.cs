using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    public class RecommenderSpreadingActivation
    {
        // starting activation value of the first vertex for the graph of spreading activation
        private const double START_ACTIVATION_VALUE = 1;

        // ratio to preserving the activating value at the vertex
        private const double MULTIPLICATOR_OF_ACTIVATION_PRESERVING = 0.5;

        private Boolean DEBUG = false;

        private int maxLevel;
        private int numberOfSimilarNeighbors;
        private int numberOfNeighborsForNextLevel;
        private SimilarityCacheModels simCacheModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxLevel">Id of the signed in user</param>
        /// <param name="numberOfNSimilarNeighbors">Number of similar neighbors to take</param>
        /// <param name="numberOfNeighborsForNextLevel">Number of neighbors for next speading level</param>
        /// <param name="model">Data model provides similarity of books</param>
        /// <returns>New instance</returns>
        public RecommenderSpreadingActivation(int maxLevel, int numberOfSimilarNeighbors,
            int numberOfNeighborsForNextLevel, SimilarityCacheModels simCacheModel)
        {
            this.maxLevel = maxLevel;
            this.numberOfSimilarNeighbors = numberOfSimilarNeighbors;
            this.numberOfNeighborsForNextLevel = numberOfNeighborsForNextLevel;
            this.simCacheModel = simCacheModel;
        }

        /// <summary>
        /// Spreading activation recommender.
        /// If the user is logged in, we remove books already rated by him from the recommendation.
        /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">Depth of activation spreading</param>
        /// <returns>List of books with the highest activation value</returns>
        public List<int> Recommend(int bookId, string userId, int howMany = 6)
        {
            // initialize the activation graph
            GraphOfBooks graph = new GraphOfBooks();
            graph.AddVertex(bookId, START_ACTIVATION_VALUE);

            List<int> bookIDs = new List<int>();
            bookIDs.Add(bookId);

            // start recursive spreading activation
            RunOneLevelBFS(maxLevel, bookIDs, graph);

            graph.RemoveVertex(bookId);
            List<Tuple<int, double>> bookIDVWithActivationValues =
                graph.ExportBookIDVerticesWithActivationValues();

            BookRecommenderContext bd = new BookRecommenderContext();
            List<Tuple<int, double>> bookIdsWithActivationValuesWithoutRated =
                removeAlreadyRatedBooks(bookIDVWithActivationValues, userId, bd);

            // sorting books according to their importance from the highest to the lowest activation value
            List<Tuple<int, double>> sortedBookIdsWithActivationValues =
                bookIdsWithActivationValuesWithoutRated.OrderBy(v => v.Item2).Reverse().ToList();

            // return sorted sublist of recommendation
            return sortedBookIdsWithActivationValues.Select(v => v.Item1).Take(howMany).ToList();
        }

       private List<Tuple<int, double>> removeAlreadyRatedBooks(
           List<Tuple<int, double>> listOfBookIDsAndTheirQuantities,
           string userId, BookRecommenderContext db)
        {
            // if user signed in, remove already rated books
            if (userId == null)
            {
                return listOfBookIDsAndTheirQuantities;
            }

            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return listOfBookIDsAndTheirQuantities;
            }

            // get all ratings
            List<BookRating> userRatings = db.Ratings
                .Where(r => r.UserId == userId).ToList();

            // remove books which user already rated
            return listOfBookIDsAndTheirQuantities.Where(
                    b => !userRatings.Select(r => r.BookId).Contains(b.Item1)
                    ).ToList();
        }

        private void RunOneLevelBFS(int levelI, List<int> bookIDsPreviousLevel,
            GraphOfBooks graph)
        {
            // end of recursion
            if (levelI == 0)
            {
                return;
            }

            SpreadingRecommenderCache model = simCacheModel.spreadingRecommenderCache;

            // get next level of similar verteces
            List<int> bookIDsForNextLevel = new List<int>();
            foreach (int bookIdFromI in bookIDsPreviousLevel)
            {
                List<Tuple<int, int>> simBooksWithQuantitiesUnsortedI =
                    model.GetSimilaritiesBooksWithQuantitiesByAll(bookIdFromI);

                List<Tuple<int, int>> simBooksWithQuantitiesSortedI =
                    simBooksWithQuantitiesUnsortedI.OrderBy(v => v.Item2).Reverse().ToList();

                List<Tuple<int, int>> simBooksWithQuantitiesI =
                    simBooksWithQuantitiesSortedI.Take(numberOfSimilarNeighbors).ToList();

                // add neighbors for next level
                bookIDsForNextLevel.AddRange(
                    simBooksWithQuantitiesI.Select(b => b.Item1));

                // vertex was inserted to graph in the last iteration
                double? activationValueObjI = graph.GetActivationValueOfVertex(bookIdFromI);
                
                int numberOfneighborsI = simBooksWithQuantitiesI.Select(b => b.Item2).Sum();

                if (numberOfneighborsI == 0)
                {continue;}

                // update current vertex
                graph.SetVertex(bookIdFromI, activationValueObjI.Value * MULTIPLICATOR_OF_ACTIVATION_PRESERVING);

                // computes difference of activation value for distribution to neighbors
                double activationValueForDistributionI =
                    activationValueObjI.Value * (1- MULTIPLICATOR_OF_ACTIVATION_PRESERVING) / numberOfneighborsI;

                // re-compute weights of neighbors
                foreach (Tuple<int, int> neighborBookIdsWithQuantityI in simBooksWithQuantitiesI)
                {
                    int neighborBookIdI = neighborBookIdsWithQuantityI.Item1;
                    double neighborQuantityI = neighborBookIdsWithQuantityI.Item2;

                    double activationValueDiff =
                        neighborQuantityI * activationValueForDistributionI;

                    graph.IncreaseActivationValueOfVertex(neighborBookIdI, activationValueDiff);
                }
            }

            List<Tuple<int, int>> bookIDsForNextLevelAndTheirQuantities = bookIDsForNextLevel.GroupBy(b => b)
                    .Select(group => new Tuple<int, int>(group.Key, group.Count())).ToList();

            List<Tuple<int, int>> sortedBookIDsForNextLevelAndTheirQuantities =
                bookIDsForNextLevelAndTheirQuantities.OrderBy(v => v.Item2).Reverse().ToList();


            List<int> bookIDsForNextLevelNoDuplicate = sortedBookIDsForNextLevelAndTheirQuantities
                .Select(b => b.Item1).Take(numberOfNeighborsForNextLevel).ToList();

            if (DEBUG)
            {
                System.Console.WriteLine($"Level {levelI} finished, next leven itemCount: {bookIDsPreviousLevel.Count}");
                graph.printTopKOfGraph(10);
            }

            // recursion for next level
            RunOneLevelBFS(levelI -1, bookIDsForNextLevelNoDuplicate, graph);
        }
    }

    class GraphOfBooks
    {
        Dictionary<int, VertexBook> bookIdsDictionary;

        public GraphOfBooks()
        {
            this.bookIdsDictionary = new Dictionary<int, VertexBook>();
        }

        public int Count()
        {
            return this.bookIdsDictionary.Count();
        } 

        public List<Tuple<int, double>> ExportBookIDVerticesWithActivationValues()
        {
            return this.bookIdsDictionary.Values.Select(v =>
                    new Tuple<int, double>(v.GetBookID(), v.GetActivationValue())).ToList();
        }

        private VertexBook GetVertex(int bookId)
        {
            bookIdsDictionary.TryGetValue(bookId, out var currentRatingOfNext);

            return currentRatingOfNext;
        }

        public double? GetActivationValueOfVertex(int bookId)
        {
            VertexBook book = GetVertex(bookId);

            if (book == null)
            {
                return null;
            }
            return book.GetActivationValue();
        }

        public void SetVertex(int bookID, double activationValue)
        {
            this.bookIdsDictionary[bookID] = new VertexBook(bookID, activationValue);
        }

        public void IncreaseActivationValueOfVertex(int bookId, double increaseAmount)
        {
            VertexBook vertex = GetVertex(bookId);
            if (vertex == null)
            {
                AddVertex(bookId, increaseAmount);
            } else {
                vertex.SetActivationValue(
                    vertex.GetActivationValue() + increaseAmount);
            }
        }

        public void AddVertex(int bookId, double activationValue)
        {
            if (GetVertex(bookId) != null)
            {
                throw new System.ArgumentException("Parameter cannot be ", "" + bookId);
            }

            VertexBook vertexBook = new VertexBook(bookId, activationValue);
            this.bookIdsDictionary.Add(bookId, vertexBook);
        }

        public void RemoveVertex(int bookId)
        {
            this.bookIdsDictionary.Remove(bookId);
        }

        public void printGraph()
        {
            List<String> listOfString = new List<String>();
            foreach (var vertexI in this.bookIdsDictionary.Values)
            {
                listOfString.Add(vertexI.GetBookID() + "");
            }
            System.Console.WriteLine("GRAPH: " + String.Join(", ", listOfString.ToArray()));
        }

        public void printTopKOfGraph(int numberOfVertices)
        {
            List<VertexBook> topKVertices = this.bookIdsDictionary.Values
                .OrderBy(v => v.GetActivationValue()).Reverse().Take(numberOfVertices).ToList();
            for (int  i = 0; i < topKVertices.Count(); i++)
            {
                VertexBook vertexI = topKVertices[i];
                System.Console.WriteLine("Book " + i + " : " + vertexI.GetBookID() + " " + vertexI.GetActivationValue());
            }
        }
        class VertexBook
        {
            int bookID;
            double activationValue;

            public VertexBook(int bookID, double activationValue)
            {
                this.bookID = bookID;
                this.activationValue = activationValue;
            }

            public int GetBookID()
            {
                return this.bookID;
            }

            public double GetActivationValue()
            {
                return this.activationValue;
            }

            public void SetActivationValue(double activationValue)
            {
                this.activationValue = activationValue;
            }

            public void IncreaseActivationValue(double increaseAmount)
            {
                this.activationValue += increaseAmount;
            }
        }

    }
}