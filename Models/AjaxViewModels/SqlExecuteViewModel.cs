using BookRecommender.DataManipulation;
using System.Collections.Generic;
using BookRecommender.Models.Database;
using System.Linq;

namespace BookRecommender.Models.AjaxViewModels
{
    public class SqlExecute
    {   
        public List<string> ColumnNames;
        public List<List<string>> Data;
    }
}