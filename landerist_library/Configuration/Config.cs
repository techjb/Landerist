﻿namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        private static readonly string VERSION_PRODUCTION = "1.00";

        public static readonly bool TRAINING_MODE = true;

        public static readonly bool SKIP_PARSE_LISTINGS = false;

        public static readonly bool INDEXER_ENABLED = false;

        // Environment.UserInteractive && !Config.IsConfigurationProduction()
        public static readonly bool LOGS_ENABLED = true;

        public static readonly bool TIMERS_ENABLED = true;

        private static readonly string VERSION_LOCAL = new Random().Next(1000, 9999).ToString();
        public static string? VERSION { get; set; }

        public const string DATABASE_NAME = PrivateConfig.DATABASE_NAME;

        public const string DATABASE_USER = PrivateConfig.DATABASE_USER;

        public const string DATABASE_PW = PrivateConfig.DATABASE_PW;

        private static readonly string CONNECTION_USER =
            "User ID=" + DATABASE_USER + ";" +
            "Password=" + DATABASE_PW + ";" +
            "Connect Timeout=100000;";

        private static readonly string DATASOURCE_LOCAL = PrivateConfig.DATASOURCE_LOCAL;

        private static readonly string DATASOURCE_PRODUCTION = PrivateConfig.DATASOURCE_PRODUCTION;

        public static string? DATASOURCE { get; set; }

        private static readonly string DATABASE_CONNECTION_LOCAL = CONNECTION_USER +
            "Data Source=" + DATASOURCE_LOCAL + ";" +
            "Initial Catalog=";

        private static readonly string DATABASE_CONNECTION_PRODUCTION = CONNECTION_USER +
            "Data Source=" + DATASOURCE_PRODUCTION + ";" +
            "Initial Catalog=";

        public static string? DATABASE_CONNECTION { get; set; }

        public static readonly string OPENAI_API_KEY = PrivateConfig.OPENAI_API_KEY;

        public static string? EXPORT_DIRECTORY { get; set; }

        public static readonly string INSERT_DIRECTORY = PrivateConfig.INSERT_DIRECTORY;

        private static readonly string EXPORT_DIRECTORY_LOCAL = PrivateConfig.EXPORT_LOCAL_DIRECTORY;

        private static readonly string EXPORT_DIRECTORY_PRODUCTION = PrivateConfig.EXPORT_PRODUCTION_DIRECTORY;

        public static readonly string DELIMITATIONS_DIRECTORY = PrivateConfig.DELIMITATIONS_DIRECTORY;

        public static readonly string USER_AGENT = "Mozilla/5.0 (compatible; AcmeInc/1.0)";

        public static readonly int HTTPCLIENT_SECONDS_TIMEOUT = 10;

        public static readonly int MAX_CRAW_DELAY_SECONDS = 60;

        public static readonly string GOOGLEMAPS_API = PrivateConfig.GOOGLEMAPS_API;

        public static readonly string GOOLZOOM_API = PrivateConfig.GOOLZOOM_API;

        public static bool IsConfigurationProduction()
        {
            return ConfigurationProduction;
        }

        public static void Init(bool configurationProduction)
        {
            ConfigurationProduction = configurationProduction;

            VERSION = ConfigurationProduction ?
                VERSION_PRODUCTION :
                VERSION_LOCAL;

            DATASOURCE = ConfigurationProduction ?
                DATASOURCE_PRODUCTION :
                DATASOURCE_LOCAL;

            DATABASE_CONNECTION = ConfigurationProduction ?
                DATABASE_CONNECTION_PRODUCTION :
                DATABASE_CONNECTION_LOCAL;

            EXPORT_DIRECTORY = configurationProduction ?
                EXPORT_DIRECTORY_LOCAL :
                EXPORT_DIRECTORY_PRODUCTION;
        }
    }
}
