using Xunit;
using System;
using BookRecommender.Models;
using BookRecommender.Models.Database;
using BookRecommender.DataManipulation;
using System.Linq;

namespace BookRecommender.Tests
{
    public class EntityFrameworkTests
    {
        [Fact]
        public void BookAuthorRelationship()
        {
            using (new TestData())
            {
                var db = new BookRecommenderContext();

                var book = GetTestBook(0, db);

                db.BooksAuthors.Add(new BookAuthor(book, GetTestAuthor(0, db)));
                db.BooksAuthors.Add(new BookAuthor(book, GetTestAuthor(1, db)));

                db.SaveChanges();

                var db2 = new BookRecommenderContext();

                var tb0 = GetTestBook(0, db2);
                var ta0 = GetTestAuthor(0, db2);
                var ta1 = GetTestAuthor(1, db2);


                Assert.True(tb0.GetAuthors(db2).Count() == 2);

                Assert.True(ta0.GetBooks(db2).Count() == 1);
                Assert.True(ta1.GetBooks(db2).Count() == 1);

                Assert.Contains(ta0, tb0.GetAuthors(db2));
                Assert.Contains(ta1, tb0.GetAuthors(db2));

                Assert.Contains(tb0, ta0.GetBooks(db2));
                Assert.Contains(tb0, ta1.GetBooks(db2));
            }
        }

        [Fact]
        public void BookCharacterRelationship()
        {
            using (new TestData())
            {
                var db = new BookRecommenderContext();

                var book = GetTestBook(0, db);

                book.AddCharacter(GetTestCharacter(0, db), db);
                book.AddCharacter(GetTestCharacter(1, db), db);

                db.SaveChanges();

                var db2 = new BookRecommenderContext();

                var tb0 = GetTestBook(0, db2);
                var tc0 = GetTestCharacter(0, db2);
                var tc1 = GetTestCharacter(1, db2);


                Assert.True(tb0.GetCharacters(db2).Count() == 2);
                Assert.True(tc0.GetBooks(db2).Count() == 1);
                Assert.True(tc1.GetBooks(db2).Count() == 1);

                Assert.Contains(tc0, tb0.GetCharacters(db2));
                Assert.Contains(tc1, tb0.GetCharacters(db2));

                Assert.Contains(tb0, tc0.GetBooks(db2));
                Assert.Contains(tb0, tc1.GetBooks(db2));
            }
        }

        [Fact]
        public void BookGenreRelationship()
        {
            using (new TestData())
            {
                var db = new BookRecommenderContext();

                var book = GetTestBook(0, db);

                book.AddGenre(GetTestGenre(0, db), db);
                book.AddGenre(GetTestGenre(1, db), db);
                db.SaveChanges();

                var db2 = new BookRecommenderContext();

                var tb0 = GetTestBook(0, db2);
                var tg0 = GetTestGenre(0, db2);
                var tg1 = GetTestGenre(1, db2);


                Assert.True(tb0.GetGenres(db2).Count() == 2);
                Assert.True(tg0.GetBooks(db2).Count() == 1);
                Assert.True(tg1.GetBooks(db2).Count() == 1);

                Assert.Contains(tg0, tb0.GetGenres(db2));
                Assert.Contains(tg1, tb0.GetGenres(db2));

                Assert.Contains(tb0, tg0.GetBooks(db2));
                Assert.Contains(tb0, tg1.GetBooks(db2));
            }
        }

        [Fact]
        public void BookTagRelationship()
        {
            using (new TestData())
            {
                var db = new BookRecommenderContext();

                var book = GetTestBook(0, db);

                book.AddTag(GetTestTag(0, db), db);
                book.AddTag(GetTestTag(1, db), db);
                db.SaveChanges();

                var db2 = new BookRecommenderContext();

                var tb0 = GetTestBook(0, db2);
                var tt0 = GetTestTag(0, db2);
                var tt1 = GetTestTag(1, db2);

                Assert.True(tb0.GetTags(db2).Count() == 2);
                Assert.True(tt0.GetBooks(db2).Count() == 1);
                Assert.True(tt1.GetBooks(db2).Count() == 1);

                Assert.Contains(tt0, tb0.GetTags(db2));
                Assert.Contains(tt1, tb0.GetTags(db2));

                Assert.Contains(tb0, tt0.GetBooks(db2));
                Assert.Contains(tb0, tt1.GetBooks(db2));
            }
        }



        // ----------------------------------------------
        // Help methods
        // ----------------------------------------------
        const string TestBook = "testBook";
        const string TestAuthor = "testAuthor";
        const string TestCharacter = "testCharacter";
        const string TestGenre = "testGenre";
        const string TestTag = "testTag";

        static void RemoveTestEntries()
        {
            // Clean database from test entries
            var db = new BookRecommenderContext();

            foreach (var item in db.Books.Where(b => b.Uri.StartsWith(TestBook)))
            {
                db.Remove(item);
            }
            foreach (var item in db.Authors.Where(a => a.Uri.StartsWith(TestAuthor)))
            {
                db.Remove(item);
            }
            foreach (var item in db.Characters.Where(c => c.Uri.StartsWith(TestCharacter)))
            {
                db.Remove(item);
            }
            foreach (var item in db.Genres.Where(g => g.Uri.StartsWith(TestGenre)))
            {
                db.Remove(item);
            }
            foreach (var item in db.Tags.Where(t => t.Value.StartsWith(TestTag)))
            {
                db.Remove(item);
            }
            db.SaveChanges();
            System.Console.WriteLine("Test entities removed");
        }

        static void AddTestEntries()
        {
            AddTestAuthors();
            AddTestBooks();
            AddTestCharacters();
            AddTestGenres();
            AddTestTags();
        }

        static void AddTestBooks()
        {
            var db = new BookRecommenderContext();

            for (int i = 0; i < 5; i++)
            {
                db.Books.Add(new Book()
                {
                    Uri = TestBook + i
                });
            }
            db.SaveChanges();
        }

        Book GetTestBook(int i, BookRecommenderContext db)
        {
            if (i < 0 || i > 5)
            {
                throw new ArgumentOutOfRangeException();
            }
            return db.Books.Where(t => t.Uri == TestBook + i).First();
        }

        static void AddTestAuthors()
        {
            var db = new BookRecommenderContext();

            for (int i = 0; i < 5; i++)
            {
                db.Authors.Add(new Author()
                {
                    Uri = TestAuthor + i
                });
            }
            db.SaveChanges();
        }
        Author GetTestAuthor(int i, BookRecommenderContext db)
        {
            if (i < 0 || i > 5)
            {
                throw new ArgumentOutOfRangeException();
            }
            var ret = db.Authors.Where(t => t.Uri == TestAuthor + i).First();
            return ret;
        }
        static void AddTestCharacters()
        {
            var db = new BookRecommenderContext();

            for (int i = 0; i < 5; i++)
            {
                db.Characters.Add(new Character()
                {
                    Uri = TestCharacter + i
                });
            }
            db.SaveChanges();
        }
        Character GetTestCharacter(int i, BookRecommenderContext db)
        {
            if (i < 0 || i > 5)
            {
                throw new ArgumentOutOfRangeException();
            }
            return db.Characters.Where(t => t.Uri == TestCharacter + i).First();
        }
        static void AddTestGenres()
        {
            var db = new BookRecommenderContext();

            for (int i = 0; i < 5; i++)
            {
                db.Genres.Add(new Genre()
                {
                    Uri = TestGenre + i
                });
            }
            db.SaveChanges();
        }
        Genre GetTestGenre(int i, BookRecommenderContext db)
        {
            if (i < 0 || i > 5)
            {
                throw new ArgumentOutOfRangeException();
            }
            return db.Genres.Where(t => t.Uri == TestGenre + i).First();
        }
        static void AddTestTags()
        {
            var db = new BookRecommenderContext();

            for (int i = 0; i < 5; i++)
            {
                db.Tags.Add(new Tag()
                {
                    Value = TestTag + i
                });
            }
            db.SaveChanges();
        }
        Tag GetTestTag(int i, BookRecommenderContext db)
        {
            if (i < 0 || i > 5)
            {
                throw new ArgumentOutOfRangeException();
            }
            return db.Tags.Where(t => t.Value == TestTag + i).First();
        }

        class TestData : IDisposable
        {
            public TestData()
            {
                AddTestEntries();
            }
            public void Dispose()
            {
                RemoveTestEntries();
            }
        }
    }
}