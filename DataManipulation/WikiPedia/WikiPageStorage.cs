using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookRecommender.DataManipulation.WikiPedia
{
    /// <summary>
    /// Storage for raw preprocessed Wikipedia pages. It uses simple UTF-8 text files
    /// together with simple directory structure. Please specify the path of the
    /// root directory for this system in the appsettings.json file.
    /// </summary>
    class WikiPageStorage
    {
        readonly string rootDir;

        public WikiPageStorage()
        {
            var dir = AppSettingsSingleton.Mining.WikiPagesStorage;
            try{
                Path.GetFullPath(dir);
            } catch (Exception e){
                throw new DirectoryNotFoundException("Directory: " + dir, e);
            }
            rootDir = dir;
            if (rootDir.Last() != Path.DirectorySeparatorChar)
            {
                rootDir += Path.DirectorySeparatorChar;
            }
        }
        /// <summary>
        /// Check if the page exists in this storage.
        /// </summary>
        /// <param name="pageId">Wikipedia page id</param>
        /// <param name="lang">Desired language</param>
        /// <returns>True if page with desired language exists.</returns>
        public virtual  bool PageExist(string pageId, string lang)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            var filePath = dirpath + pageId;
            return File.Exists(filePath);
        }
        /// <summary>
        /// Command to remove page from storage
        /// </summary>
        /// <param name="pageId">Wikipedia page id</param>
        /// <param name="lang"></param>
        public virtual  void RemovePage(string pageId, string lang)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            var filePath = dirpath + pageId;
            File.Delete(filePath);
        }
        /// <summary>
        /// Saves new page to the storage
        /// </summary>
        /// <param name="page">Page data</param>
        /// <param name="lang">Page language</param>
        /// <param name="pageId">Page id</param>
        /// <returns>True if the operation succeeds</returns>
        public virtual  bool SavePage(string page, string lang, string pageId)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dirpath))
            {
                Directory.CreateDirectory(dirpath);
            }
            var filePath = dirpath + pageId;
            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, page);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                return false;
            }
        }
        /// <summary>
        /// Retrieves specific page.
        /// </summary>
        /// <param name="lang">Page lang</param>
        /// <param name="pageId">Page id</param>
        /// <returns>Data for the page, or null if the page does not exists</returns>
        public virtual  string GetPage(string lang, string pageId)
        {
            var filePath = rootDir + lang + Path.DirectorySeparatorChar + pageId;
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }
        /// <summary>
        /// Returns the enumeration of all page ids in language.
        /// </summary>
        /// <param name="lang">Language to retrive</param>
        /// <return>Return list of pages in specified language.</return>
        public virtual  IEnumerable<(string id, string text)> GetPagesInLang(string lang)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dirpath))
            {
                yield break;
            }
            foreach (var file in Directory.GetFiles(dirpath))
            {
                var id = new FileInfo(file).Name;
                yield return (id, File.ReadAllText(file));
            }
        }

        /// <summary>
        /// Count how many pages in language do we store.
        /// </summary>
        /// <param name="lang">Which language to count</param>
        /// <returns>Number of pages in language</returns>
        public virtual  int PagesInLangCount(string lang){
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dirpath))
            {
                return 0;
            }
            return Directory.GetFiles(dirpath).Length;
        }
        /// <summary>
        /// Retrieves all languages in which we store data
        /// </summary>
        /// <returns>Enum of all languages</returns>
        public virtual  IEnumerable<string> GetLangs()
        {
            return Directory.GetDirectories(rootDir).Select(d => new DirectoryInfo(d).Name);
        }
    }
}