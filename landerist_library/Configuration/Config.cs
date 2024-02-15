﻿namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        public static readonly string VERSION = "1.44";

        public static readonly bool SET_LATLNG_LAUID_AND_MEDIA_TO_LISTING = true;

        public static readonly bool INDEXER_ENABLED = true;

        public static readonly bool WORDS_ENABLED = false;

        public static readonly int MAX_PAGES_PER_WEBSITE = 100;

        public static readonly int MAX_PAGES_PER_SCRAPE = 10000;

        public static int MIN_PAGES_PER_SCRAPE { get; set; }

        private const int MIN_PAGES_PER_SCRAPE_PRODUCTION = 20;

        private const int MIN_PAGES_PER_SCRAPE_LOCAL = 0;

        public static readonly int MAX_PAGES_PER_HOSTS_PER_SCRAPE = 5;

        public static readonly int MAX_SITEMAPS_PER_WEBSITE = 10;

        public static readonly int DAYS_TO_UPDATE_ROBOTS_TXT = 3;

        public static readonly int DAYS_TO_UPDATE_SITEMAP = 3;

        public static readonly int DAYS_TO_UPDATE_IP_ADDRESS = 7;

        //public static readonly int MAX_LISTINGS_PER_WEBSITE = 100;

        public static readonly int MAX_RESPONSEBODYTEXT_LENGTH = 10000;

        public static readonly int MAX_PAGETYPE_COUNTER = 1000;

        public static readonly int MIN_RESPONSEBODYTEXT_LENGTH = 50;
        public static bool SCRAPE_WITH_PARALELISM { get; set; }

        public static readonly bool LOGS_ENABLED = true;
        public static bool TIMERS_ENABLED { get; set; }

        public static readonly string USER_AGENT = "Mozilla/5.0 (compatible; AcmeInc/1.0)";

        public static readonly int HTTPCLIENT_SECONDS_TIMEOUT = 10;

        public static readonly int MAX_CRAW_DELAY_SECONDS = 120;

        public static readonly double MINIMUM_PERCENTAGE_TO_BE_SIMILAR_PAGE = 0.85;

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

        public static readonly string MLMODEL_DIRECTORY = PrivateConfig.MLMODEL_DIRECTORY;

        public static readonly string MLMODEL_TRAINING_DATA_DIRECTORY = MLMODEL_DIRECTORY + @"TrainingData\";

        public static readonly string CLASSIFIER_DIRECTORY = PrivateConfig.CLASIFFIER_DIRECTORY;

        public static readonly string DELIMITATIONS_DIRECTORY = PrivateConfig.DELIMITATIONS_DIRECTORY;
        public static string? BACKUPS_DIRECTORY { get; set; }

        public static readonly string BACKUPS_LOCAL_DIRECTORY = PrivateConfig.BACKUPS_LOCAL_DIRECTORY;

        public static readonly string BACKUPS_PRODUCTION_DIRECTORY = PrivateConfig.BACKUPS_PRODUCTION_DIRECTORY;

        public static readonly string GOOGLE_CLOUD_LANDERIST_API_KEY = PrivateConfig.GOOGLE_GLOUD_LANDERIST_API_KEY;

        public static readonly string GOOGLE_SEARCH_ENGINE_ID = PrivateConfig.GOOGLE_SEARCH_ENGINE_ID;

        public static readonly string GOOGLE_NATURAL_LANGUAGE_API_KEY = PrivateConfig.GOOGLE_NATURAL_LANGUAGE_API_KEY;

        public static readonly string GOOLZOOM_API = PrivateConfig.GOOLZOOM_API;

        public static readonly string AWS_ACESSKEYID = PrivateConfig.AWS_ACESSKEYID;

        public static readonly string AWS_SECRETACCESSKEY = PrivateConfig.AWS_SECRETACCESSKEY;

        public static readonly string AWS_S3_PUBLIC_BUCKET = PrivateConfig.AWS_S3_BUCKET_PUBLIC;

        public static readonly string AWS_S3_BUCKET_BACKUPS = PrivateConfig.AWS_S3_BUCKET_BACKUPS;

        public static readonly string IDAGENCIES_URL = PrivateConfig.IDAGENCIES_URL;

        public static readonly int DAYS_TO_DELETE_BACKUP = 60;

        public static bool IsConfigurationProduction()
        {
            return ConfigurationProduction;
        }

        public static void SetToProduction()
        {
            Init(true);
        }

        public static void SetDatabaseToProduction()
        {
            InitDatabase(true);
        }

        public static void SetToLocal()
        {
            Init(false);
        }

        private static void Init(bool configurationProduction)
        {
            ConfigurationProduction = configurationProduction;

            InitDatabase(configurationProduction);

            EXPORT_DIRECTORY = ConfigurationProduction ?
                EXPORT_DIRECTORY_PRODUCTION :
                EXPORT_DIRECTORY_LOCAL;

            BACKUPS_DIRECTORY = ConfigurationProduction ?
                BACKUPS_PRODUCTION_DIRECTORY :
                BACKUPS_LOCAL_DIRECTORY;

            MIN_PAGES_PER_SCRAPE = ConfigurationProduction ?
                MIN_PAGES_PER_SCRAPE_PRODUCTION :
                MIN_PAGES_PER_SCRAPE_LOCAL;

            TIMERS_ENABLED = !ConfigurationProduction;

            SCRAPE_WITH_PARALELISM = ConfigurationProduction;
        }

        private static void InitDatabase(bool configurationProduction)
        {
            DATASOURCE = configurationProduction ?
                DATASOURCE_PRODUCTION :
                DATASOURCE_LOCAL;

            DATABASE_CONNECTION = configurationProduction ?
                DATABASE_CONNECTION_PRODUCTION :
                DATABASE_CONNECTION_LOCAL;
        }
    }
}
