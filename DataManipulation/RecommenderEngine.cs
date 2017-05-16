using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation
{
    class RecommenderEngine
    {
        public List<int> RecommendBookSimilar(int bookId, string userId = null, int howMany = 6)
        {
            // TODO: nenabizet jiz ratovane knihy



            // MUCH OPTIMISATION AVAILABLE

            var timer = new Stopwatch();

            timer.Start();

            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }

            // get data from the book we want to be the center of a search
            var authors = myBook.GetAuthors(db);
            var genres = myBook.GetGenres(db);
            var characters = myBook.GetCharacters(db);
            var publisher = myBook.Publisher;
            var lang = myBook.OrigLang;



            var listOfBooks = new List<int>();

            foreach (var author in authors)
            {
                listOfBooks.AddRange(author.GetBooksIds(db));
            }

            foreach (var genre in genres)
            {
                listOfBooks.AddRange(genre.GetBooksIds(db));
            }

            foreach (var character in characters)
            {
                listOfBooks.AddRange(character.GetBooksIds(db));
            }

            listOfBooks.AddRange(db.Books.Where(b => b.Publisher == publisher).Select(b => b.BookId));
            listOfBooks.AddRange(db.Books.Where(b => b.OrigLang == lang).Select(b => b.BookId));

            var groupedList = listOfBooks.GroupBy(b => b).
                     Select(group =>
                         new
                         {
                             BookId = group.Key,
                             Count = group.Count()
                         });
            var sortedList = groupedList.OrderByDescending(b => b.Count).Select(b => b.BookId);


            // if user signed in, remove already rated books
            if (userId != null)
            {
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    // get all ratings
                var userRatings = db.Ratings.Where(r => r.UserId == userId).ToList();

                // remove books which user already rated
                sortedList = sortedList.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b)).ToList();
                }
            }


            var recList = sortedList.Take(howMany + 1).ToList();

            // remove myself
            var wasRemoved = recList.Remove(bookId);

            if (!wasRemoved)
            {
                recList.RemoveAt(recList.Count - 1);
            }

            System.Console.WriteLine("--- ELAPSED TIME:" + timer.ElapsedMilliseconds);

            return recList;
        }

        internal List<int> RecommendMostPopular(int howMany, string userId = null)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var mostPopularBooks = db.Ratings
                             .OrderByDescending(r => r.CreatedTime)
                             .Take(1000)
                             .Select(r => new {Id = r.BookId, Rating = r.Rating})
                             .ToList();


            var groupedList = mostPopularBooks.GroupBy(r => r.Id).
                     Select(group =>
                         new
                         {
                             BookId = group.Key,
                             Score = group.Sum(i => i.Rating)
                         });
            var sortedList = groupedList.OrderByDescending(b => b.Score).Select(b => b.BookId);

            if (userId != null)
            {
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    // get all ratings
                var userRatings = db.Ratings.Where(r => r.UserId == userId).ToList();

                // remove books which user already rated
                sortedList = sortedList.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b));
                }
            }

            return sortedList.Take(howMany).ToList();
        }

        public List<int> RecommendBookSimilarByTags(int bookId, string userId = null, int howMany = 6)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var myBook = db.Books.Find(bookId);
            if (myBook == null)
            {
                return null;
            }
            var tags = myBook.GetTags(db).Where(t => t.Score != null);


            var tagsLanguages = TopLang(tags.Select(t => t.Lang).Distinct());
            // var tagsLanguages = new string[] {"en"};

            // for each language find the most matching books by tags and then apply the merging function

            var listTopFromAllLangs = new List<(string lang, int bookId, double score)>();

            foreach (var lang in tagsLanguages)
            {
                var tagsInLang = tags.Where(t => t.Lang == lang);

                var simTagsQuery = new List<ItemWeight<int>>();

                foreach (var tag in tagsInLang)
                {
                    var simTags = db.Tags.Where(t => (t.Value == tag.Value && t.Lang == lang && t.Score != null)).Select(t =>
                    new ItemWeight<int>(
                        t.BookId,
                        t.Score.Value * tag.Score.Value));
                    simTagsQuery.AddRange(simTags);
                }
                // generate top matches for lang
                var orderedFinalScore = simTagsQuery.GroupBy(t => t.itemId).Select(g => new
                {
                    BookId = g.Key,
                    FinalScore = g.Sum(s => s.itemWeight)
                }).OrderByDescending(t => t.FinalScore);

                // take top 10 from language and add it to final list
                var top = orderedFinalScore.Take(10);
                foreach (var score in top)
                {
                    listTopFromAllLangs.Add((lang, score.BookId, score.FinalScore));
                }
            }
            // Final aggregation between languages

            var finalList = listTopFromAllLangs.GroupBy(s => s.bookId).Select(
                g => new
                {
                    BookId = g.Key,
                    Score = g.Sum(s => PenalizeLang(s.score, s.lang))
                }
            ).OrderByDescending(t => t.Score).Select(t => t.BookId);

            // if user signed in, remove already rated books
            if (userId != null)
            {
                var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    // get all ratings
                var userRatings = db.Ratings.Where(r => r.UserId == userId).ToList();

                // remove books which user already rated
                finalList = finalList.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b)).ToList();
                }
            }

            var recList = finalList.Take(howMany + 1).ToList();

            // remove myself
            var wasRemoved = recList.Remove(bookId);
            if (!wasRemoved && recList.Count > 0)
            {
                recList.RemoveAt(recList.Count - 1);
            }
            return recList;
        }
        private double PenalizeLang(double score, string lang)
        {
            //TODO: IMPLEMENT LANGUAGE PENALIZATION
            return score;
        }
        private string[] TopLang(IEnumerable<string> langs)
        {
            var db = new BookRecommenderContext();

            var langCounts = db.TagsCount.ToArray();

            return langs.Select(t => new
            {
                Count = langCounts.Where(c => c.Lang == t).FirstOrDefault().Count,
                Lang = t
            })
                        .OrderByDescending(r => r.Count)
                        .Take(5)
                        .Select(q => q.Lang)
                        .ToArray();
        }

        // due to a bug in value tuple, created as a hotfix for method below
        struct HelpStruct
        {
            public int bookId;
            public string userId;
            public int rating;
        }

        public List<int> RecommendForUserUBased(string userId, int howMany = 6)
        {
            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // get all positive ratings
            var userRatings = db.Ratings.Where(r => r.UserId == userId && r.Rating >= 3).Select(r => new
            {
                Rating = r.Rating,
                Book = r.BookId
            }).ToList();

            // load all ratings for same books
            //var simUserRatings = new List<(int bookId, string userId, int rating)>();
            // var simUserRatings = new List<(int bookId, string userId, int rating)>();
            var simUserRatings = new List<HelpStruct>();


            foreach (var rating in userRatings)
            {
                var simRatings = db.Ratings.Where(r => r.BookId == rating.Book && r.UserId != userId).ToList();
                var s = simRatings.Select(r =>
                new HelpStruct
                {
                    bookId = r.BookId,
                    userId = r.UserId,
                    rating = r.Rating
                });
                if (simRatings != null && simRatings.Any())
                {
                    simUserRatings.AddRange(s);
                }
            }

            // count book averages for users that has not rated the book
            var bookAvg = simUserRatings.GroupBy(r => r.bookId).Select(group =>
            new
            {
                BookId = group.Key,
                AvgScore = group.Sum(s => s.rating) / group.Count()
            }
            );



            //count similarities - use book avg if rating not available

            var simUsers = simUserRatings.GroupBy(r => r.userId).Select(group =>
            new
            {
                UserId = group.Key,
                // we want to evaluate every book for every user, so we will use bookAvg as a linq base element in score count
                // because it has all books
                Score = bookAvg.Sum(b =>
                {
                    int scoreA = userRatings.First(ra => ra.Book == b.BookId).Rating;

                    //if sim has data, use it, else use default from bookAvg
                    var hasData = group.Any(p => p.bookId == b.BookId);
                    var scoreB = hasData ? group.First(p => p.bookId == b.BookId).rating : b.AvgScore;
                    //count metrics
                    return Math.Pow(scoreA - scoreB, 2);
                })
            }
            );

            var closestFourUsers = simUsers.OrderBy(u => u.Score).Take(4).ToList();
            var recommendedBooks = new List<int>();


            foreach (var score in new int[] { 5, 4, 3 })
            {
                foreach (var closestUser in closestFourUsers)
                {
                    var potentialBooks = db.Ratings.Where(r => r.UserId == closestUser.UserId && r.Rating == score).ToList();
                    var pBNotYetSeen = potentialBooks.Where(b => !userRatings.Select(r => r.Book).Contains(b.BookId)).ToList();
                    recommendedBooks.AddRange(pBNotYetSeen.Select(b => b.BookId));
                    if (recommendedBooks.Distinct().Count() >= howMany)
                    {
                        return recommendedBooks.Distinct().ToList();
                    }
                }
            }
            return recommendedBooks.Distinct().ToList();
        }


        struct ItemWeight<T>
        {
            public ItemWeight(T itemId, double itemWeight)
            {
                this.itemId = itemId;
                this.itemWeight = itemWeight;
            }
            public T itemId;
            public double itemWeight;
        }
        public List<int> RecommendForUserCBased(string userId, int howMany = 6)
        {
            var howManyLastItems = 5;

            var db = new BookRecommenderContext();
            db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            var booksToFindSimilarityFor = new List<ItemWeight<int>>();

            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // get all positive ratings
            var userRatings = db.Ratings.Where(r => r.UserId == userId && r.Rating >= 3).ToList();


            var userRatingNormalized = userRatings.OrderByDescending(o => o.CreatedTime).Take(howManyLastItems).Select(r =>
            new ItemWeight<int>
            {
                itemId = r.BookId,
                itemWeight = r.Rating - 2
            }).ToList();

            booksToFindSimilarityFor.AddRange(userRatingNormalized);

            var userActionsBookViewed = db.UsersActivities.Where(ac => ac.UserId == userId &&
                                                            ac.Type == UserActivity.ActivityType.BookDetailViewed)
                                                .OrderByDescending(a => a.CreatedTime)
                                                .Take(howManyLastItems).ToArray();

            var userActionsBookViewedModified = userActionsBookViewed.Select(b =>
                                                     new ItemWeight<int>
                                                     {
                                                         itemId = int.Parse(b.Value),
                                                         itemWeight = 1
                                                     }).ToList();

            booksToFindSimilarityFor.AddRange(userActionsBookViewedModified);

            booksToFindSimilarityFor = booksToFindSimilarityFor.Distinct().ToList();

            var authors = new List<ItemWeight<int>>();
            var genres = new List<ItemWeight<int>>();
            var characters = new List<ItemWeight<int>>();
            var tags = new List<ItemWeight<Tag>>();

            foreach (var book in booksToFindSimilarityFor)
            {
                var queriedAuthors = db.BooksAuthors.Where(ba => ba.BookId == book.itemId);
                authors.AddRange(queriedAuthors.Select(qa => new ItemWeight<int>(qa.AuthorId, book.itemWeight)));

                var queriedGenres = db.BooksGenres.Where(bg => bg.BookId == book.itemId);
                genres.AddRange(queriedGenres.Select(qg => new ItemWeight<int>(qg.GenreId, book.itemWeight)));

                var queriedCharacters = db.BooksCharacters.Where(ba => ba.BookId == book.itemId);
                characters.AddRange(queriedCharacters.Select(qc => new ItemWeight<int>(qc.CharacterId, book.itemWeight)));

                // use only en lang for simplification, at book level, multiple languages are used
                var queriedTags = db.Tags.Where(ba => ba.BookId == book.itemId && ba.Lang == "en");
                tags.AddRange(queriedTags.Select(qt => new ItemWeight<Tag>(qt, book.itemWeight)));
            }

            // agregate preferences to possibly meke the next queries smaller
            authors = authors.GroupBy(g => g.itemId).Select(group =>
                                new ItemWeight<int>(group.Key, group.Sum(i => i.itemWeight)))
                                .ToList();
            genres = genres.GroupBy(g => g.itemId).Select(group =>
                                new ItemWeight<int>(group.Key, group.Sum(i => i.itemWeight)))
                                .ToList();
            characters = characters.GroupBy(g => g.itemId).Select(group =>
                                new ItemWeight<int>(group.Key, group.Sum(i => i.itemWeight)))
                                .ToList();
            tags = tags.GroupBy(g => g.itemId).Select(group =>
                                new ItemWeight<Tag>(group.Key, group.Sum(i => i.itemWeight)))
                                .ToList();



            var candidateBooks = new List<ItemWeight<int>>();
            var candidateBooksByTags = new List<ItemWeight<int>>();


            foreach (var author in authors)
            {
                var books = db.BooksAuthors.Where(ba => ba.AuthorId == author.itemId).Select(ba => ba.BookId);
                candidateBooks.AddRange(books.Select(b => new ItemWeight<int>(b, author.itemWeight)));
            }

            foreach (var genre in genres)
            {
                var books = db.BooksGenres.Where(bg => bg.GenreId == genre.itemId).Select(ba => ba.BookId);
                candidateBooks.AddRange(books.Select(b => new ItemWeight<int>(b, genre.itemWeight)));
            }

            foreach (var character in characters)
            {
                var books = db.BooksCharacters.Where(bc => bc.CharacterId == character.itemId).Select(ba => ba.BookId);
                candidateBooks.AddRange(books.Select(b => new ItemWeight<int>(b, character.itemWeight)));
            }

            foreach (var tag in tags)
            {
                var simTags = db.Tags.Where(t => t.Value == tag.itemId.Value && t.TagId != tag.itemId.TagId).ToArray();
                candidateBooksByTags.AddRange(simTags.Select(
                        st =>
                        new ItemWeight<int>(
                                st.BookId,
                                st.Score.GetValueOrDefault() * tag.itemId.Score.GetValueOrDefault() * tag.itemWeight)));
            }

            // remove books which user already rated
            candidateBooks = candidateBooks.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b.itemId)).ToList();

            var candidateBooksFinalOrdered = candidateBooks.GroupBy(g => g.itemId)
                                                    .Select(group =>
                                                        new ItemWeight<int>(
                                                            group.Key,
                                                            group.Sum(i => i.itemWeight)
                                                        ))
                                                    .OrderByDescending(o => o.itemWeight)
                                                    .Take(howMany).Select(b => b.itemId);


            var candidateBooksByTagsFinalOrdered = candidateBooksByTags.GroupBy(g => g.itemId)
                                                    .Select(group =>
                                                        new ItemWeight<int>(
                                                            group.Key,
                                                            group.Sum(i => i.itemWeight)
                                                        ))
                                                    .OrderByDescending(o => o.itemWeight)
                                                    .Take(howMany).Select(b => b.itemId);

            return candidateBooksFinalOrdered.Take(howMany / 2)
                                              .Concat(candidateBooksByTagsFinalOrdered.Take(howMany / 2))
                                              .ToList();
        }
    }
}