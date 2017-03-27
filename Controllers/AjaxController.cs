using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookRecommender.Models;
using BookRecommender.DataManipulation;

namespace BookRecommender.Controllers
{
    public class AjaxController : Controller
    {
        public IActionResult SparqlData(string entityUri){
            var additionalData = DataMiner.GetAdditionalData(entityUri);
            return PartialView("AdditionalSparqlData", additionalData);
        }
        public string DynamicImage(string entityUri){
            var db = new BookRecommenderContext();
            IGoogleImg dbObj = db.Books.Where(b => b.Uri  == entityUri)?.FirstOrDefault();
            if(dbObj == null){
                dbObj = db.Authors.Where(a => a.Uri  == entityUri)?.FirstOrDefault();
            }
            if(dbObj == null){
                return "";
            }
            return dbObj.TryToGetImgUrl();
        }
    }
}
