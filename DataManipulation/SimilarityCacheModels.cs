using System.Collections.Generic;
using System.Linq;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.DataManipulation
{
    public class SimilarityCacheModels
    {
        public readonly SpreadingRecommenderCache spreadingRecommenderCache;

        public SimilarityCacheModels(
            SpreadingRecommenderCache spreadingRecommenderCache)
        {
            this.spreadingRecommenderCache = spreadingRecommenderCache;
        }

    }
}