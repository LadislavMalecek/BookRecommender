
using Microsoft.Extensions.Configuration;

namespace BookRecommender
{
    public static class AppSettingsSingleton
    {
        public static class Mining
        {
            public static string WikiPagesStorage;
            public static string Password;
        }
        public static class Database
        {
            //default for creating the database file
            public static string Connection = "Data Source=./BookRecommender.db;";
        }
    }
}