using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BookRecommender.Models.Database;
using BookRecommender.Models.AjaxViewModels;
using System.Text;
using Newtonsoft.Json;
using static BookRecommender.DataManipulation.MiningStateType;
using Microsoft.AspNetCore.Authorization;

namespace BookRecommender.Controllers
{
    public class AjaxController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AjaxController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> SparqlData(string entityUri)
        {
            var additionalData = await DataMiner.GetAdditionalDataAsync(entityUri);
            return PartialView("AdditionalSparqlData", additionalData);
        }
        public async Task<string> DynamicImage(string entityUri)
        {
            var db = new BookRecommenderContext();
            IGoogleImg dbObj = db.Books.Where(b => b.Uri == entityUri)?.FirstOrDefault();
            if (dbObj == null)
            {
                dbObj = db.Authors.Where(a => a.Uri == entityUri)?.FirstOrDefault();
            }
            if (dbObj == null)
            {
                return "";
            }
            return await dbObj.TryToGetImgUrlAsync();
        }
        public string[] QueryAutoComplete(string query)
        {
            // return "ahoj jak se vede".Split(' ');
            return SearchEngine.Autocomplete(new BookRecommenderContext(), query, 10).ToArray();
        }
        public IActionResult Recommendation(string type, int data, int howMany = 6)
        {
            IEnumerable<int> recList;
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
            }

            switch (type)
            {
                case "bookPage":
                    recList = new RecommenderEngine().RecommendBookSimilar(data, userId, howMany);
                    break;
                case "bookPageByTags":
                    recList = new RecommenderEngine().RecommendBookSimilarByTags(data, userId, howMany);
                    break;
                case "userBased":
                    recList =
                       userId != null
                       ? new RecommenderEngine().RecommendForUserUBased(userId, howMany)
                       : null;
                    break;
                case "contentBased":
                    recList =
                        userId != null
                        ? new RecommenderEngine().RecommendForUserCBased(userId, howMany)
                        : null;
                    break;
                case "mostPopular":
                    recList = new RecommenderEngine().RecommendMostPopular(howMany, userId);
                    break;
                default:
                    return null;
            }

            if (recList == null)
            {
                recList = new List<int>();
            }

            var recommendations = new List<Recommendation>();
            recommendations = recList.Select(r => new Recommendation(r)).ToList();

            return PartialView("Recommendation", recommendations);
        }
        public class JsonHelp
        {
            public string status;
            public string message;
            public JsonHelp(string status, string message)
            {
                this.status = status;
                this.message = message;
            }
        }
        [Authorize]
        public string ManageSync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            var jsonDic = new Dictionary<string, JsonHelp>();
            var miningProxy = DataMiningProxySingleton.Instance;
            foreach (var operation in miningProxy.Operations)
            {
                var id = operation.UniqueId;
                var status = operation.state.CurrentState;
                var message = operation.state.Message;

                // remove null value
                message = message == null ? "" : message;

                // add mining progress as a message
                jsonDic.Add(id, new JsonHelp(status.ToString(), message));
            }
            return (JsonConvert.SerializeObject(jsonDic));
        }
        [Authorize]
        public void Mining(string command, string param)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return;
            }
            if (string.IsNullOrEmpty(command))
            {
                return;
            }
            switch (command)
            {
                case "start":
                    if (param != null)
                    {
                        var isValid = Guid.TryParse(param, out Guid guid);
                        if (isValid)
                        {
                            DataMiningProxySingleton.Instance.AddForProccessing(param);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        DataMiningProxySingleton.Instance.MineAll();
                    }
                    break;
                case "stop":
                    if (param != null)
                    {
                        var isValid = Guid.TryParse(param, out Guid guid);
                        if (isValid)
                        {
                            DataMiningProxySingleton.Instance.RemoveFromProccessing(param);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        DataMiningProxySingleton.Instance.RemoveAll();
                    }
                    break;
                default:
                    return;
            }
        }
        [Authorize]
        public void Feedback(string text, string name){
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
            } else {
                return;
            }
            if(string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(name)){
                return;
            }
            var db = new BookRecommenderContext();

            db.Feedback.Add(new Feedback(userId,text,name));
            db.SaveChanges();
        }
    }
}
