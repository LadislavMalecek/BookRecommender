using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BookRecommender.DataManipulation;

namespace BookRecommender.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public List<(string endpointName, List<(string Name, string Id)> operations)> OperationsByEndpoint;
        public readonly static int PageSize = 10;
        public int FeedbackPageCount;


        public IndexViewModel()
        {
            var db = new BookRecommenderContext();
            // how many pages
            FeedbackPageCount = (db.Feedback.Count() + PageSize - 1) / PageSize;

            OperationsByEndpoint = new List<(string endpointName, List<(string Name, string Id)> operations)>();
            var miningProxy = DataMiningProxySingleton.Instance;
            
            // add all operations to list
            foreach (var operation in miningProxy.Operations)
            {
                var endpointName = operation.endpoint?.GetName();
                var OpTransformed = (operation.name, operation.UniqueId);
                var list = OperationsByEndpoint.Where(o => o.endpointName == endpointName);
                if (list.Count() == 0)
                {
                    OperationsByEndpoint.Add((endpointName, new List<(string, string)>(){
                        OpTransformed
                    }));
                }
                else
                {
                    list.First().operations.Add(OpTransformed);
                }
            }
        }
    }
}
