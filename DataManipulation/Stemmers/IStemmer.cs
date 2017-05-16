

namespace BookRecommender.DataManipulation.Stemmers
{
    /// <summary>
    /// Interface used for creating new stemmers
    /// </summary>
    interface IStemmer
    {
        string StemWord(string word);
    }
}