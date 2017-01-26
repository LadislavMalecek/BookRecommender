

namespace BookRecommender.DataManipulation{
    interface IMiner
    {
        SparqlResult MineData(string query);
    }
}