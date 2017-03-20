

namespace BookRecommender.DataManipulation.Stemmers
{
    interface IStemmer
    {
        string StemWord(string word);
    }
}