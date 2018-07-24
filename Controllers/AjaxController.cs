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
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.Controllers
{
    /// <summary>
    /// Controller that is responsible for all AJAX calls issued from the client jQuery script calls
    /// /Ajax/
    /// </summary>
    public class AjaxController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AjaxController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Ajax call that is responsible for loading additional SPARQL data ond book and author detail pages
        /// </summary>
        /// <param name="entityUri">Url of the entity we want to load additional data about</param>
        /// <returns>html of the response which can be inserted in the page</returns>
        public async Task<IActionResult> SparqlData(string entityUri)
        {
            var additionalData = await DataMiner.GetAdditionalDataAsync(entityUri);
            return PartialView("AdditionalSparqlData", additionalData);
        }

        /// <summary>
        /// Ajax call that is responsible for handling the dynamic loading of images from database.
        /// When the picture is not in the database, we try to obtain the picture from Google image search.
        /// If loaded from Google, caching will occur.
        /// </summary>
        /// <param name="entityUri">Url of the entity for which we want to load the image</param>
        /// <returns>Url of the image</returns>
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
        /// <summary>
        /// Ajax call that is responsible for handling the help whisperer list generated under the search bar.
        /// Directly calls Search engine to retrive simpler version of search.
        /// </summary>
        /// <param name="query">Query that is the subject of the search</param>
        /// <returns>Array of the resulted whispered items</returns>
        public string[] QueryAutoComplete(string query)
        {
            // return "ahoj jak se vede".Split(' ');
            return SearchEngine.Autocomplete(new BookRecommenderContext(), query, 10).ToArray();
        }

        /// <summary>
        /// Ajax call that handles the recommendation calls from the client.
        /// </summary>
        /// <param name="type">Type of the recommendation - bookPage, bookPageByTags, userBased, contentBased, mostPopular</param>
        /// <param name="data">Parameters for the recommendation - bookPage, bookPageByTags: id of the book</param>
        /// <param name="howMany">How many items should be retrieved</param>
        /// <returns>Formated HTML that can be inserted into the page</returns>
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
                case "spreadingActivations":
                    recList = new SpreadingRecommenderEngine().RecommendBooksSimilarBySpreadingActivation(new List<int>() { data }, howMany).Select(b => b.BookId);
                    break;
                default:
                    return null;
            }

            if (recList == null)
            {
                recList = new List<int>();
            }

            var recommendations = new List<Recommendation>();
            recommendations = recList.Select(r => new Recommendation(r)).Take(howMany).ToList();

            return PartialView("Recommendation", recommendations);
        }

        // object that will be send in a list to the client at ManageSync()
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

        /// <summary>
        /// It checks the state of miner and returns the information back to the user.
        /// Ajax call only available to signed in users with admin privilege.
        /// </summary>
        /// <returns>Simple Json format with data about the mining engine. </returns>
        [Authorize]
        public string ManageSync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }
            bool hasAccess = _userManager.GetUserAsync(HttpContext.User).Result.HasManageAccess;
            if (!hasAccess)
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

        /// <summary>
        /// Ajax call for issuing a new commands to the mining engine.
        /// Only available to signed users with admin privilege.
        /// </summary>
        /// <param name="command">2 commands available - start, stop.</param>
        /// <param name="param">Parameter of the command, unique id of the operation that we want to execute.</param>
        [Authorize]
        public void Mining(string command, string param)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return;
            }

            bool hasAccess = _userManager.GetUserAsync(HttpContext.User).Result.HasManageAccess;
            if (!hasAccess)
            {
                return;
            }

            if (string.IsNullOrEmpty(command))
            {
                return;
            }
            var paramIsValid = param != null && Guid.TryParse(param, out Guid guid);
            switch (command)
            {
                case "start":
                    if (paramIsValid)
                    {
                        DataMiningProxySingleton.Instance.AddForProccessing(param);
                    }
                    else
                    {
                        DataMiningProxySingleton.Instance.MineAll();
                    }
                    break;
                case "stop":
                    if (paramIsValid)
                    {
                        DataMiningProxySingleton.Instance.RemoveFromProccessing(param);
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
        /// <summary>
        /// Ajax call for saving feedback from the user.
        /// Only available to logged in users.
        /// </summary>
        /// <param name="text">Text of the feedback</param>
        /// <param name="name">Name of the person giving the feedback</param>

        [Authorize]
        public void Feedback(string text, string name)
        {
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
            }
            else
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            var db = new BookRecommenderContext();

            db.Feedback.Add(new Feedback(userId, text, name));
            db.SaveChanges();
        }

        /// <summary>
        /// Ajax call that load feedback for it to be shown at a manage page.
        /// User has to be signed in with administrators privilege.
        /// </summary>
        /// <param name="page">Page number of feedback list to be returned</param>
        /// <returns>Formated HTML table with feedback values</returns>
        [Authorize]
        public IActionResult GetFeedback(int page)
        {
            if (page < 1)
            {
                return StatusCode(400);
            }
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }

            bool hasAccess = _userManager.GetUserAsync(HttpContext.User).Result.HasManageAccess;
            if (!hasAccess)
            {
                return null;
            }

            int howManySkip = (page - 1) * Models.ManageViewModels.IndexViewModel.PageSize;
            //return Authors?.Skip(howManySkip)?.Take(PageSize)

            var feedbackList = new List<(string name, string feedback, string email)>();
            var db = new BookRecommenderContext();
            db.Feedback.OrderByDescending(f => f.CreatedTime)
                       .Include(x => x.User)
                       .Skip(howManySkip)?
                       .Take(Models.ManageViewModels.IndexViewModel.PageSize)?
                       .ToList()
                       .ForEach(f =>
                       {
                           feedbackList.Add((f.Name, f.Text, f.User.Email));
                       });

            var model = new GetFeedback()
            {
                Data = feedbackList
            };

            return PartialView(model);
        }

        /// <summary>
        /// Ajax call to return the custom SQL command issued from the manage page.
        /// Only available to signed in users with manage privileges.
        /// Security critical!!!
        /// </summary>
        /// <param name="query">SQL query to be executed</param>
        /// <returns>Formated HTML table with the result</returns>
        [Authorize]
        public IActionResult SqlExecute(string query)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }

            bool hasAccess = _userManager.GetUserAsync(HttpContext.User).Result.HasManageAccess;
            if (!hasAccess)
            {
                return null;
            }
            // ToDo: move to separate class as a new object
            using (var context = new BookRecommenderContext())
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                context.Database.OpenConnection();

                // execute the query
                using (var rdr = command.ExecuteReader())
                {
                    // get names of columns
                    var columns = new List<string>();
                    var data = new List<List<string>>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        columns.Add(rdr.GetName(i));
                    }

                    // get values
                    while (rdr.Read())
                    {
                        var row = new List<string>();
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            row.Add(rdr.GetValue(i).ToString());
                        }
                        data.Add(row);
                    }
                    var model = new SqlExecute()
                    {
                        ColumnNames = columns,
                        Data = data
                    };
                    return PartialView(model);
                }
            }
        }
    }
}
