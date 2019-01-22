using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;

namespace BookRecommender.DataManipulation.Recommender
{
    class RecommenderUserBasedKNN
    {

        /// <summary>
        /// We will first gather the users preferences and the find books that will match the most,
        /// search is similar to the single book centric recommendation in method RecommendBookSimilar
        /// the difference is at the start, where we first gather the information from user history.
        /// </summary>
        /// <param name="userId">Id of the signed in user</param>
        /// <param name="howMany">How many books to return</param>
        /// <returns>List of recommended books</returns>
        public static List<int> Recommend(string userId, int howMany = 6)
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


            // each rated book will contribute to the score 1,2 or 3 points
            var userRatingNormalized = userRatings.OrderByDescending(o => o.CreatedTime).Take(howManyLastItems).Select(r =>
            new ItemWeight<int>
            {
                itemId = r.BookId,
                itemWeight = r.Rating - 2
            }).ToList();

            booksToFindSimilarityFor.AddRange(userRatingNormalized);

            // every action will contribute with the weight of one
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

            // now just get the similarity properties 
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

            // add to the candidate books books that have similar attributes to our preferred 
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

            // add similar books by tags to separate list, the weights are too small to be mixed, the tag based information would be lost
            foreach (var tag in tags)
            {
                var simTags = db.Tags.Where(t => t.Value == tag.itemId.Value && t.TagId != tag.itemId.TagId).ToArray();
                candidateBooksByTags.AddRange(simTags.Select(
                        st =>
                        new ItemWeight<int>(
                                st.BookId,
                                st.Score.GetValueOrDefault() * tag.itemId.Score.GetValueOrDefault() * tag.itemWeight)));
            }

            // can be moved down to improve performance
            // remove books which user already rated
            candidateBooks = candidateBooks.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b.itemId)).ToList();

            candidateBooksByTags = candidateBooksByTags.Where(b => !userRatings
                                                        .Select(r => r.BookId)
                                                        .Contains(b.itemId)).ToList();
            
            // group books by id and sum the scores, pick the ones with biggest weight
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

            // combine the best candidates from both lists
            return candidateBooksFinalOrdered.Take(howMany / 2)
                                              .Concat(candidateBooksByTagsFinalOrdered.Take(howMany / 2))
                                              .ToList();
        }
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

}