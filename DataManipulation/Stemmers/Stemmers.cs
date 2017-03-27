namespace BookRecommender.DataManipulation.Stemmers
{
    class Stemmers
    {
        public static IStemmer GetStemmerToLang(string lang)
        {
            switch (lang)
            {
                case "en":
                    return new EnglishStemmer();
                default:
                    return null;
            }
        }
    }
}