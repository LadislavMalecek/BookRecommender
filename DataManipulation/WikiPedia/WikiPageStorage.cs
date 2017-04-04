using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookRecommender.DataManipulation.WikiPedia
{
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
        public bool PageExist(string pageId, string lang)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            var filePath = dirpath + pageId;
            return File.Exists(filePath);
        }
        public void RemovePage(string pageId, string lang)
        {
            var dirpath = rootDir + lang + Path.DirectorySeparatorChar;
            var filePath = dirpath + pageId;
            File.Delete(filePath);
        }
        public bool SavePage(string page, string lang, string pageId)
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
        public string GetPage(string lang, string pageId)
        {
            var filePath = rootDir + lang + Path.DirectorySeparatorChar + pageId;
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }
        public IEnumerable<(string id, string text)> GetPagesInLang(string lang)
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
        public IEnumerable<string> GetLangs()
        {
            return Directory.GetDirectories(rootDir).Select(d => new DirectoryInfo(d).Name);
        }
        public static string GetLangFromWikiUrl(string url)
        {
            // Example:
            // https://en.wikipedia.org/wiki/God_in_the_Age_of_Science%3F
            var splittedUrl = url?.Split(new char[] { '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return splittedUrl.Length >= 1 ? splittedUrl[1] : null;
        }
        public static string GetFileNameFromUrl(string url)
        {
            var lastSlash = url.LastIndexOf('/');
            var firstQuestionMark = url.IndexOf('?');
            if (lastSlash == -1 || lastSlash == url.Length - 1)
            {
                // very likely not valid address
                return null;
            }
            string trimmedUrl;
            if (firstQuestionMark != -1)
            {
                trimmedUrl = url.Substring(lastSlash + 1, firstQuestionMark - lastSlash - 1);
            }
            else
            {
                trimmedUrl = url.Substring(lastSlash + 1);
            }
            if (trimmedUrl.Length > 150)
            {
                // max length of file is 260 chars, Files that over reached this length are probably not that important
                // because they are probably in some minor language
                trimmedUrl = trimmedUrl.Substring(0, 150);
            }
            return trimmedUrl;
        }
    }
}