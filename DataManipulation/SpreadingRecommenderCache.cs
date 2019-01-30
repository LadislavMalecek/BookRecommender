using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;


namespace BookRecommender.DataManipulation
{
    public class SpreadingRecommenderCache
    {
        private Dictionary<int, List<int>> SimilarBooksByAuthor;
        private Dictionary<int, List<int>> SimilarBooksByTags;
        private Dictionary<int, List<int>> SimilarBooksByCharacters;

        // was too memory ineficient, there is only smaller amount of genres, but for each genre there is hude amount of books
        // private Dictionary<int, List<int>> SimilarBooksByGenre;

        private Dictionary<int, List<int>> BooksGenres;
        private Dictionary<int, List<int>> GenresBooks;

        private Dictionary<int, string> BooksNames;

        public void Initialize(BookRecommenderContext db)
        {
            var sw = Stopwatch.StartNew();

            InitializeAuthors(db);
            InitializeGenres(db);
            InitializeCharacters(db);
            InitializeTags(db);

            InitializeNames(db);

            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            System.Console.WriteLine($"Spreading cache entries: {SimilarBooksByAuthor.Keys.Count}#");
            System.Console.WriteLine($"Spreading cache init took: {elapsed}ms");
        }

        public void InitializeAuthors(BookRecommenderContext db)
        {
            // Book to authors
            var bookAuthorList = db.BooksAuthors.Select(t => new { t.BookId, t.AuthorId })
                .ToList();

            var bookToAuthors = bookAuthorList.GroupBy(ba => ba.BookId)
                .ToDictionary(g => g.Key, g => g.Select(ba => ba.AuthorId).ToList());

            var authorToBooks = bookAuthorList.GroupBy(ba => ba.AuthorId)
                .ToDictionary(g => g.Key, g => g.Select(ba => ba.BookId).ToList());

            SimilarBooksByAuthor = bookToAuthors
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                        .SelectMany(a => authorToBooks[a])
                        .ToList());

            var total = SimilarBooksByAuthor.SelectMany(f => f.Value).Count();
            var aproxMemMB = total * 4d / 1024 / 1024;
            System.Console.WriteLine($"Authors total hops available: {total}, MB: {aproxMemMB}");
        }

        // public void InitializeGenres(BookRecommenderContext db)
        // {
        //     // Book to genres
        //     var bookGenreList = db.BooksGenres.Select(t => new { t.BookId, t.GenreId })
        //         .ToList();

        //     var bookToGenres = bookGenreList.GroupBy(bg => bg.BookId)
        //                     .ToDictionary(g => g.Key, g => g.Select(bg => bg.GenreId).ToList());

        //     var genresToBooks = bookGenreList.GroupBy(bg => bg.GenreId)
        //         .ToDictionary(g => g.Key, g => g.Select(bg => bg.BookId).ToList());

        //     SimilarBooksByGenre = bookToGenres
        //         .ToDictionary(
        //             kvp => kvp.Key,
        //             kvp => kvp.Value
        //                 .SelectMany(g => genresToBooks[g])
        //                 .ToList());

        //     var total = SimilarBooksByGenre.SelectMany(f => f.Value).Count();
        //     var aproxMemMB = total * 4d / 1024 / 1024;
        //     System.Console.WriteLine($"Genres total hops available: {total}, MB: {aproxMemMB}");
        // }


        public void InitializeGenres(BookRecommenderContext db)
        {
            // Book to genres
            var bookGenreList = db.BooksGenres.Select(t => new { t.BookId, t.GenreId })
                .ToList();

            BooksGenres = bookGenreList.GroupBy(bg => bg.BookId)
                            .ToDictionary(g => g.Key, g => g.Select(bg => bg.GenreId).ToList());

            GenresBooks = bookGenreList.GroupBy(bg => bg.GenreId)
                .ToDictionary(g => g.Key, g => g.Select(bg => bg.BookId).ToList());


            var total = BooksGenres.SelectMany(f => f.Value).Count()
                + GenresBooks.SelectMany(f => f.Value).Count();

            var aproxMemMB = total * 4d / 1024 / 1024;
            System.Console.WriteLine($"Genres total hops available(2 tables): {total}, MB: {aproxMemMB}");
        }

        public void InitializeCharacters(BookRecommenderContext db)
        {
            // Book to genres
            var bookCharacterList = db.BooksCharacters.Select(t => new { t.BookId, t.CharacterId })
                .ToList();

            var bookToCharacters = bookCharacterList.GroupBy(bc => bc.BookId)
                            .ToDictionary(g => g.Key, g => g.Select(bc => bc.CharacterId).ToList());

            var charactersToBooks = bookCharacterList.GroupBy(bc => bc.CharacterId)
                .ToDictionary(g => g.Key, g => g.Select(bc => bc.BookId).ToList());

            SimilarBooksByCharacters = bookToCharacters
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                        .SelectMany(g => charactersToBooks[g])
                        .ToList());

            var total = SimilarBooksByCharacters.SelectMany(f => f.Value).Count();
            var aproxMemMB = total * 4d / 1024 / 1024;
            System.Console.WriteLine($"Characters total hops available: {total}, MB: {aproxMemMB}");
        }

        public void InitializeTags(BookRecommenderContext db)
        {
            // Book to tags
            var bookTagList = db.Tags.Select(t => new { t.BookId, t.TagId })
                .ToList();

            var bookToTags = bookTagList.GroupBy(bt => bt.BookId)
                            .ToDictionary(g => g.Key, g => g.Select(bt => bt.TagId).ToList());

            var tagsToBooks = bookTagList.GroupBy(bt => bt.TagId)
                .ToDictionary(g => g.Key, g => g.Select(bt => bt.BookId).ToList());

            SimilarBooksByTags = bookToTags
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                        .SelectMany(t => tagsToBooks[t])
                        .ToList());

            var total = SimilarBooksByTags.SelectMany(f => f.Value).Count();
            var aproxMemMB = total * 4d / 1024 / 1024;
            System.Console.WriteLine($"Tags total hops available: {total}, MB: {aproxMemMB}");
        }

        public void InitializeNames(BookRecommenderContext db)
        {
            var books = db.Books.ToList();
            BooksNames = books.ToDictionary(b => b.BookId, b => b.GetNameEn());
        }



        public List<int> GetSimilarBooksByAuthors(int bookId)
        {
            return SimilarBooksByAuthor.GetValueOrDefault(bookId, new List<int>());
        }

        // public List<int> GetSimilarBooksByGenres(int bookId)
        // {
        //     return SimilarBooksByGenre.GetValueOrDefault(bookId, new List<int>());
        // }
        public List<int> GetSimilarBooksByGenres(int bookId)
        {
            return BooksGenres
                .GetValueOrDefault(bookId, new List<int>())
                .SelectMany(g => GenresBooks.GetValueOrDefault(g, new List<int>()))
                .ToList();
        }

        public List<int> GetSimilarBooksByCharacters(int bookId)
        {
            return SimilarBooksByCharacters.GetValueOrDefault(bookId, new List<int>());
        }

        public List<int> GetSimilarBooksByTags(int bookId)
        {
            return SimilarBooksByTags.GetValueOrDefault(bookId, new List<int>());
        }

        public List<int> GetSimilarBooksByAll(int bookId)
        {
            var byAuthors = GetSimilarBooksByAuthors(bookId);
            var byGenres = GetSimilarBooksByGenres(bookId);
            var byCharacters = GetSimilarBooksByCharacters(bookId);
            var byTags = GetSimilarBooksByTags(bookId);


            var all = byAuthors.Concat(byGenres).Concat(byCharacters).Concat(byTags).ToList();

            return all;
        }

        public List<Tuple<int, int>> GetSimilaritiesBooksWithQuantitiesByAll(int bookIDI)
        {
            List<int> listOfBookIDs = GetSimilarBooksByAll(bookIDI);

            List<Tuple<int, int>> listOfBookIDsAndTheirQuantities = listOfBookIDs.GroupBy(b => b)
                    .Select(group => new Tuple<int, int>(group.Key, group.Count())).ToList();

            return listOfBookIDsAndTheirQuantities;
        }

        public string GetName(int bookId)
        {
            return BooksNames[bookId];
        }
    }
}