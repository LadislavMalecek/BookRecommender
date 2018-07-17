using System.Threading.Tasks;

namespace BookRecommender.Models.Database
{
    interface IGoogleImg
    {
        Task<string> TryToGetImgUrlAsync();
    }
}