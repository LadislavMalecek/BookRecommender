
using Microsoft.Extensions.Configuration;

namespace BookRecommender{
    public static class AppSettingsSingleton{

        // public static void Initialize(IConfigurationRoot config){
        static AppSettingsSingleton(){

            // DataBaseConnectionString = config["Database:connection"];
            // Mining.WikiPagesStorage = config["Mining:wikiPagesStorage"];
            System.Console.WriteLine("App settings singleton initialized");
        }
        public static class Mining{
            public static string WikiPagesStorage = "C:\\netcore\\booksWikiPages\\";
        }
        public static string DataBaseConnectionString  = "Filename=C://netcore//SQLite//BookRecommender.db;cache=shared";
    }
}