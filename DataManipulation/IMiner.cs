

namespace BookRecommender.DataManipulation{
    interface IMiner
    {
        string MineData(string query);
        SparqlData MineDataParse(string query);
    }
}