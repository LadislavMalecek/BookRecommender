using System.Threading.Tasks;

namespace BookRecommender.Models
{
    interface IGoogleImg
    {
        Task<string> TryToGetImgUrlAsync();
    }
}