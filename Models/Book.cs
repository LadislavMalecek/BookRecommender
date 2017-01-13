using System;

namespace BookRecommender.Models{

    public class Book{
        public int Id;
        public string Name;
        public Author Author;
        public DateTime PublicationDate { get; set; }

        
    }

}