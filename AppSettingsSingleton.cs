
using Microsoft.Extensions.Configuration;

namespace BookRecommender{
    public static class AppSettingsSingleton{
        public static void Initialize(IConfigurationRoot config){

            DataBaseConnectionString = config["Database:connection"];
            Mining.WikiPagesStorage = config["Mining:wikiPagesStorage"];
            
            System.Console.WriteLine("App settings singleton initialized");
        }
        public static class Mining{
            public static string WikiPagesStorage;
        }
        public static string DataBaseConnectionString;
    }
}