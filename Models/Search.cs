using System;
using System.Collections.Generic;
using System.Linq;

namespace BookRecommender.Models
{
    public class Search
    {
        public Search(string searchPhrase, int page, List<Book> books, List<Author> authors){
            this.SearchPhrase = searchPhrase;
            this.Page = page;
            this.Books = books;
            this.Authors = authors;
        }
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

        public IEnumerable<Book> GetBooksToShow()
        {
            if (Page < 0 || Page > HighestPage)
            {
                return Enumerable.Empty<Book>();
            }

            int howManySkip = (Page - 1) * PageSize;
            return Books?.Skip(howManySkip)?.Take(PageSize);
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
    }
}