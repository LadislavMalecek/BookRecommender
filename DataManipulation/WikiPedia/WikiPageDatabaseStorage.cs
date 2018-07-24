using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookRecommender.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BookRecommender.DataManipulation.WikiPedia
{
    /// <summary>
    /// Storage for raw preprocessed Wikipedia pages. It uses simple UTF-8 text files
    /// together with simple directory structure. Please specify the path of the
    /// root directory for this system in the appsettings.json file.
    /// </summary>
    class WikiPageDatabaseStorage : WikiPageStorage
    {
        // readonly string rootDir;
        BookRecommenderContext db;
        HashSet<(string pageId, string lang)> existingPages;

        public WikiPageDatabaseStorage()
        {
            db = new BookRecommenderContext();
            db.Database.EnsureCreated();

            existingPages = db.WikiStorage.ToList().Select(s => (s.Id, s.Lang)).ToHashSet();
        }
        /// <summary>
        /// Check if the page exists in this storage.
        /// </summary>
        /// <param name="pageId">Wikipedia page id</param>
        /// <param name="lang">Desired language</param>
        /// <returns>True if page with desired language exists.</returns>
        public override bool PageExist(string pageId, string lang)
        {
            return existingPages.Contains((pageId, lang));
        }
        /// <summary>
        /// Command to remove page from storage
        /// </summary>
        /// <param name="pageId">Wikipedia page id</param>
        /// <param name="lang"></param>
        public override void RemovePage(string pageId, string lang)
        {
            var page = db.WikiStorage.Find(pageId, lang);
            if (page != null)
            {
                existingPages.Remove((pageId, lang));
                db.WikiStorage.Remove(page);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Saves new page to the storage
        /// </summary>
        /// <param name="page">Page data</param>
        /// <param name="lang">Page language</param>
        /// <param name="pageId">Page id</param>
        /// <returns>True if the operation succeeds</returns>
        public override bool SavePage(string text, string lang, string pageId, bool saveToDb = true)
        {
            var alreadyAdded = existingPages.Contains((pageId, lang));
            if (!alreadyAdded)
            {
                db.WikiStorage.Add(new WikiStorageEntry(pageId, lang, text));
                if (saveToDb)
                {
                    db.SaveChanges();
                }
                return true;
            }
            return false;
        }


        public async Task<bool> SavePageAsync(string text, string lang, string pageId, bool saveToDb = true)
        {
            var alreadyAdded = existingPages.Contains((pageId, lang));
            if (!alreadyAdded)
            {
                db.WikiStorage.Add(new WikiStorageEntry(pageId, lang, text));
                if (saveToDb)
                {
                    await db.SaveChangesAsync();
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Retrieves specific page.
        /// </summary>
        /// <param name="lang">Page lang</param>
        /// <param name="pageId">Page id</param>
        /// <returns>Data for the page, or null if the page does not exists</returns>
        public override string GetPage(string lang, string pageId)
        {
            return db.WikiStorage.Find(pageId, lang)?.Text;
        }
        /// <summary>
        /// Returns the enumeration of all page ids in language.
        /// </summary>
        /// <param name="lang">Language to retrive</param>
        /// <return>Return list of pages in specified language.</return>
        public override IEnumerable<(string id, string text)> GetPagesInLang(string lang)
        {
            return db.WikiStorage.Where(ws => ws.Lang == lang).ToList().Select(ws => (ws.Id, ws.Text)).ToList();
        }

        /// <summary>
        /// Count how many pages in language do we store.
        /// </summary>
        /// <param name="lang">Which language to count</param>
        /// <returns>Number of pages in language</returns>
        public override int PagesInLangCount(string lang)
        {
            return db.WikiStorage.Where(ws => ws.Lang == lang).Count();
        }
        /// <summary>
        /// Retrieves all languages in which we store data
        /// </summary>
        /// <returns>Enum of all languages</returns>
        public override IEnumerable<string> GetLangs()
        {
            return db.WikiStorage.Select(ws => ws.Lang).Distinct().ToList();
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}