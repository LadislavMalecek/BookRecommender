

namespace BookRecommender.DataManipulation{

    /// <summary>
    /// Interface to be used with custom instances of SPARQL data downloader.
    /// </summary>
    interface IDownloader
    {
        string MineData(string query);

    }
}