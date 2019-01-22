using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderDiversityEnhancedBookSimilarity
    {
        static int CORE_SIZE = 10;
        static int CANDIDATES_SIZE = 90;

        /// <summary>
        /// Recommender algorithm Maximal Marginal Relevance (MMR) searchs books similar to one other,
        /// at the same time strives for diversity of returned books.
        /// Algorithm is based od ContentBasedBookSimilarity recommender. In the first step, the algorithm
        /// requires a list of all similar books to one other. Then it select the first 10 (CORE_SIZE)
        /// books as a core of resulting recommendation list. The other 90 (CANDIDATES_SIZE) books from
        /// the list are taken as candidates. The algorithm then iteratively adds one candidate to the
        /// result list. Another element of the resulting list is selected to maximize diversity
        /// (minimizing the similarity of books in the resulting list).
        /// The similarity of books is measured by Jaccard's similarity to the authors and the genres.
        /// /// </summary>
        /// <param name="bookId">Id of book on which will the recommendation be based</param>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>Maximal Marginal Relevance List of books (bookIDs) based on results of
        /// RecommenderBookSimilar algorithm
        /// /// </returns>
        public static List<int> Recommend(int bookId, string userId = null,
            double lambda = 0.2, int howMany = 6)
        {
            List<Tuple<int, int>> bookIDsAndTheirQuantitiesAll =
                RecommenderContentBasedBookSimilarity.RecommendWeightedList(bookId, userId);

            // creating core and candidates of books
            List<Tuple<int, int>> recCoreWList =
                bookIDsAndTheirQuantitiesAll.Take(CORE_SIZE).ToList();
            List<Tuple<int, int>> recCandidatesWList =
                bookIDsAndTheirQuantitiesAll.Skip(CORE_SIZE).Take(CANDIDATES_SIZE).ToList();


            // creating similarity model of books
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            List<int> boodIdsForModel = new List<int>();
            boodIdsForModel.AddRange(recCoreWList.Select(b => b.Item1).ToList());
            boodIdsForModel.AddRange(recCandidatesWList.Select(b => b.Item1).ToList());

            BooksSimilarityModel model = new BooksSimilarityModel(boodIdsForModel);
            model.CountSimilarity(db);


            // run diversity enhanced recommender
            List<int> recCoreList = recCoreWList.Select(b => b.Item1).ToList();

            List<Tuple<int, double>> recCandidatesNormWList =
                NormBookIDsAndTheirQuantities(recCandidatesWList);

            return RecommenderDiversityEnhanced.Recommend(recCoreList,
                recCandidatesNormWList, model, lambda, howMany).ToList();
        }

        private static List<Tuple<int, double>> NormBookIDsAndTheirQuantities(
            List<Tuple<int, int>> bookIDsAndTheirQuantities)
        {
            if (bookIDsAndTheirQuantities.Count() == 0)
            {
                return new List<Tuple<int, double>>();
            }

            int max = bookIDsAndTheirQuantities.Select(b => b.Item2).Max();
            
            return bookIDsAndTheirQuantities.Select(
                b => new Tuple<int, double>(b.Item1, (double)b.Item2/max)).ToList();
        }
    }

    class RecommenderDiversityEnhanced
    {
        public static List<int> Recommend(List<int> recCoreOfBookIDs,
            List<Tuple<int, double>> bookIDsAvailableForAdding,
            BooksSimilarityModel model, double lambda, int howMany)
        {
            // iterative addition available books to rec. core
            List<int> currentlySelectedBookIDs = recCoreOfBookIDs;

            while (currentlySelectedBookIDs.Count() < howMany) {

                Tuple<int, double> nextBookIDAndWeight = SelectNextBook(
                    currentlySelectedBookIDs, bookIDsAvailableForAdding,
                    lambda, model);
                if (nextBookIDAndWeight == null) {
                    return currentlySelectedBookIDs;
                }

                currentlySelectedBookIDs.Add(nextBookIDAndWeight.Item1);
                bookIDsAvailableForAdding.Remove(nextBookIDAndWeight);
            }

            return currentlySelectedBookIDs.Take(howMany).ToList();
        }

        private static Tuple<int, double> SelectNextBook(List<int> currentlySelectedBookIDs,
            List<Tuple<int, double>> currentlyAvailableBookIDAndWeightsForSelection,
            double lambda, BooksSimilarityModel model) {

            if (currentlyAvailableBookIDAndWeightsForSelection.Count == 0) {
                return null;
            }

            double theBestSimilarity = Double.MinValue;
            Tuple<int, double> theBestBookIDAndWeight = null;

            foreach (Tuple<int, double> bookIDAndWeightI in currentlyAvailableBookIDAndWeightsForSelection)
            {
                int bookIDI = bookIDAndWeightI.Item1;
                double relevancyI = bookIDAndWeightI.Item2;

                double similarityMaxI = model.GetSimilarityMax(
                    currentlySelectedBookIDs, bookIDI);

                double similarityI =
                    lambda * relevancyI - (1 - lambda) * similarityMaxI;
                
                if (similarityI > theBestSimilarity)
                {
                    theBestSimilarity = similarityI;
                    theBestBookIDAndWeight = bookIDAndWeightI;
                }
            }

            return theBestBookIDAndWeight;
        }

    }

    class BooksSimilarityModel
    {
        private double[,] matrix;
        private List<int> bookIDs;

        public BooksSimilarityModel(List<int> bookIDs)
        {
            this.matrix = new double[bookIDs.Count(), bookIDs.Count()];
            this.bookIDs = bookIDs;
        }

        public void CountSimilarity(BookRecommenderContext db)
        {
            List<BookSimilarityInformation> books = new List<BookSimilarityInformation>();

            foreach (int bookIDI in bookIDs) {

                Book bookI = db.Books.Find(bookIDI);
                List<Author> authorsI = bookI.GetAuthors(db).ToList();
                List<Genre> genresI = bookI.GetGenres(db).ToList();

                BookSimilarityInformation bookInfoI =
                    new BookSimilarityInformation(bookI.BookId, authorsI, genresI);

                books.Add(bookInfoI);
            }

            for (int bookIndex1 = 0; bookIndex1 < books.Count; bookIndex1++)
            {
                for (int bookIndex2 = 0; bookIndex2 < books.Count; bookIndex2++)
                {
                    if (bookIndex1 + bookIndex2 >= books.Count)
                    {
                        continue;
                    }

                    BookSimilarityInformation bookInfo1 = books[bookIndex1];
                    BookSimilarityInformation bookInfo2 = books[bookIndex2];

                    double simValue = Similarity(bookInfo1, bookInfo2);
                    this.matrix[bookIndex1, bookIndex2] = simValue;
                    this.matrix[bookIndex2, bookIndex1] = simValue;                        
                }
            }

        }

        public double GetSimilarityMax(List<int> bookIDs, int bookID)
        {
            double max = -1;
            foreach (int bookIDI in bookIDs)
            {
                double simI = GetSimilarity(bookIDI, bookID);
                if (simI > max)
                    {max = simI;}
            }
            return max;
        }

        public double GetSimilaritySum(List<int> bookIDs, int bookID)
        {
            double sum = 0;
            foreach (int bookIDI in bookIDs)
            {
                sum += GetSimilarity(bookIDI, bookID); 
            }
            return sum;
        }

        public double GetSimilarity(int bookID1, int bookID2)
        {
            int bookIndex1 = IndexOf(bookID1);
            int bookIndex2 = IndexOf(bookID2);

            return this.matrix[bookIndex1, bookIndex2];
        }

        private int IndexOf(int bookID)
        {
            return bookIDs.IndexOf(bookID);
        }

        private double Similarity(BookSimilarityInformation bookInfo1,
            BookSimilarityInformation bookInfo2)
        {
            List<Author> authors1 = bookInfo1.authors;
            List<Genre> genres1 = bookInfo1.genres;

            List<Author> authors2 = bookInfo2.authors;
            List<Genre> genres2 = bookInfo2.genres;

            double similarity = 0;
            similarity += JaccardSimilarity(authors1, authors2);
            similarity += JaccardSimilarity(genres1, genres2);

            return similarity;
        }

        private double JaccardSimilarity(List<Author> authors1,
            List<Author> authors2)
        {
            int intersectionSize = 0;
            foreach (Author authorI in authors2)
            {
                if (authors1.ToList().Contains(authorI, new AuthorComparer())) {
                    intersectionSize++;
                }
            }

            int unionSize = authors1.Count() + authors2.Count() - intersectionSize;

            return (double)intersectionSize / unionSize;
        }

        private double JaccardSimilarity(List<Genre> genres1,
            List<Genre> genres2)
        {
            int intersectionSize = 0;
            foreach (Genre genreI in genres2)
            {
                if (genres1.ToList().Contains(genreI, new GenreComparer())) {
                    intersectionSize++;
                }
            }

            int unionSize = genres1.Count() + genres2.Count() - intersectionSize;

            return (double)intersectionSize / unionSize;
        }


        class BookSimilarityInformation
        {
            public int bookID;
            public List<Author> authors;
            public List<Genre> genres;

            public BookSimilarityInformation(int bookID,
                List<Author> authors, List<Genre> genres)
            {
                this.bookID = bookID;
                this.authors = authors;
                this.genres = genres;
            }
        }

        class AuthorComparer : IEqualityComparer<Author>
        {
            public bool Equals(Author x, Author y)
            {
                return x.AuthorId == y.AuthorId;
            }

            public int GetHashCode(Author obj)
            {
                return obj.AuthorId;
            }
        }

        class GenreComparer : IEqualityComparer<Genre>
        {
            public bool Equals(Genre x, Genre y)
            {
                return x.NameEn == y.NameEn;
            }

            public int GetHashCode(Genre obj)
            {
                return obj.NameEn.GetHashCode();
            }
        }

    }

}