
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
            public static string Connection;
        }
    }
}