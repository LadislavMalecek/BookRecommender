using System;
using System.Collections.Generic;
using System.Linq;
using BookRecommender.DataManipulation;
using BookRecommender.Models.Database;


namespace BookRecommender.Models.HomeViewModels
{
    public class SearchViewModel
    {
        public SearchViewModel(string searchPhrase, int page, List<Book> books, List<Author> authors, BookRecommenderContext db)
        {
            this.SearchPhrase = searchPhrase;
            this.Page = page;
            this.Books = books;
            this.Authors = authors;
            this.db = db;
        }
        BookRecommenderContext db;
        readonly int PageSize = 20;
        public string SearchPhrase { get; private set; }
        List<Book> Books;
        List<Author> Authors;
        public int Page { get; private set; }
        public int HighestPage
        {
            get
            {
                var sizeMax = Math.Max(Books.Count, Authors.Count);
                return (int)Math.Ceiling((double)sizeMax / PageSize);
            }
        }

        public IEnumerable<BookHelpClass> GetBooksToShow()
        {
            if (Page < 0 || Page > HighestPage)
            {
                return Enumerable.Empty<BookHelpClass>();
            }

            int howManySkip = (Page - 1) * PageSize;
            return Books?.Skip(howManySkip)
                        ?.Take(PageSize)
                        ?.Select(b => new BookHelpClass(b, db));
        }
        public IEnumerable<Author> GetAuthorsToShow()
        {
            if (Page < 0 || Page > HighestPage)
            {
                return Enumerable.Empty<Author>();
            }

            int howManySkip = (Page - 1) * PageSize;
            return Authors?.Skip(howManySkip)?.Take(PageSize);
        }

        public bool ShowPrevious()
        {
            if (Page > 1)
            {
                return true;
            }
            return false;
        }
        public bool ShowNext()
        {
            if (Page < HighestPage)
            {
                return true;
            }
            return false;
        }

        public int TotalBooksFound
        {
            get
            {
                return Books.Count;
            }
        }
        public int TotalAuthorsFound
        {
            get
            {
                return Authors.Count;
            }
        }
        public class BookHelpClass
        {
            public BookHelpClass(Book book, BookRecommenderContext db)
            {
                Book = book;
                BookAuthors = book.GetAuthors(db);
                BookGenres = book.GetGenres(db);
                BookRating = book.GetRating(db);
            }
            public Book Book { get; private set; }
            public IEnumerable<Author> BookAuthors { get; private set; }
            public IEnumerable<Genre> BookGenres { get; private set; }
            public int? BookRating { get; private set; }

        }
    }
}