using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;
using System.Threading.Tasks;

namespace BookRecommender.Controllers
{
    public class AjaxController : Controller
    {
        public async Task<IActionResult> SparqlData(string entityUri){
            var additionalData = await DataMiner.GetAdditionalDataAsync(entityUri);
            return PartialView("AdditionalSparqlData", additionalData);
        }
        public async Task<string> DynamicImage(string entityUri){
            var db = new BookRecommenderContext();
            IGoogleImg dbObj = db.Books.Where(b => b.Uri  == entityUri)?.FirstOrDefault();
            if(dbObj == null){
                dbObj = db.Authors.Where(a => a.Uri  == entityUri)?.FirstOrDefault();
            }
            if(dbObj == null){
                return "";
            }
            return await dbObj.TryToGetImgUrlAsync();
        }
        public string[] QueryAutoComplete(string query){
            // return "ahoj jak se vede".Split(' ');
            return SearchEngine.Autocomplete(new BookRecommenderContext(), query, 10).ToArray();
        }
        public async Task<IActionResult> Recommendation(){
            var data = new List<Recommendation>(){
                new Recommendation(57679),
                new Recommendation(57927),
                new Recommendation(57953),
                new Recommendation(57954),
                new Recommendation(57955),
                new Recommendation(57966)
            };
            return PartialView("Recommendation", data);
        }
    }
}
