using BookRecommender.DataManipulation;
using System.Collections.Generic;
using BookRecommender.Models.Database;
using System.Linq;

namespace BookRecommender.Models.AjaxViewModels
{
    public class GetFeedback
    {
        public List<(string name, string feedback, string email)> Data;
    }
}