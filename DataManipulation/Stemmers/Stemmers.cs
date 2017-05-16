namespace BookRecommender.DataManipulation.Stemmers
{
    /// <summary>
    /// Class for routing requests from languages to the specific Stemmers.
    /// After adding new Stemmer, it has to be added here.
    /// </summary>
    class Stemmers
    {
        /// <summary>
        /// Gets available stemmer from language code
        /// </summary>
        /// <param name="lang">code of language</param>
        /// <returns>Specifics stemmer or null if stemmer does not exists</returns>
        public static IStemmer GetStemmerToLang(string lang)
        {
            switch (lang.ToLower())
            {
                case "en":
                    return new EnglishStemmer();
                default:
                    return null;
            }
        }
    }
}