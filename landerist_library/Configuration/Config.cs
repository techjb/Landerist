namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        private static readonly string VERSION_PRODUCTION = "1.00";

        public static readonly bool SET_LATLNG_LAUID_AND_MEDIA_TO_LISTING = true;

        public static readonly bool STORE_RESPONSE_BODY_TEXT_IN_DATABASE = false;

        public static readonly bool INDEXER_ENABLED = true;

        public static readonly bool WORDS_ENABLED = false;

        public static readonly int MAX_PAGES_PER_WEBSITE = 100;

        public static readonly int PAGES_PER_SCRAPE = 10000;

        //public static readonly int MAX_LISTINGS_PER_WEBSITE = 100;

        public static readonly int MAX_RESPONSEBODYTEXT_LENGTH = 10000;

        public static readonly int MAX_PAGE_TYPE_COUNTER = 1000;

        public static readonly int MIN_RESPONSEBODYTEXT_LENGTH = 50;

        public static readonly bool LOGS_ENABLED = true;

        public static readonly bool TIMERS_ENABLED = true;

        public static readonly string USER_AGENT = "Mozilla/5.0 (compatible; AcmeInc/1.0)";

        public static readonly int HTTPCLIENT_SECONDS_TIMEOUT = 10;

        public static readonly int MAX_CRAW_DELAY_SECONDS = 120;

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

        public static readonly string MLMODEL_DIRECTORY = PrivateConfig.MLMODEL_DIRECTORY;

        public static readonly string MLMODEL_TRAINING_DATA_DIRECTORY = MLMODEL_DIRECTORY + @"TrainingData\";

        public static readonly string CLASSIFIER_DIRECTORY = PrivateConfig.CLASIFFIER_DIRECTORY;

        public static readonly string DELIMITATIONS_DIRECTORY = PrivateConfig.DELIMITATIONS_DIRECTORY;

        public static readonly string BACKUPS_DIRECTORY = PrivateConfig.BACKUPS_DIRECTORY;

        public static readonly string GOOGLE_MAPS_API = PrivateConfig.GOOGLE_MAPS_API;

        public static readonly string GOOGLE_NATURAL_LANGUAGE_API_KEY = PrivateConfig.GOOGLE_NATURAL_LANGUAGE_API_KEY;

        public static readonly string GOOLZOOM_API = PrivateConfig.GOOLZOOM_API;

        public static readonly string AWS_ACESSKEYID = PrivateConfig.AWS_ACESSKEYID;

        public static readonly string AWS_SECRETACCESSKEY = PrivateConfig.AWS_SECRETACCESSKEY;

        public static readonly string AWS_S3_PUBLIC_BUCKET = PrivateConfig.AWS_S3_BUCKET_PUBLIC;

        public static readonly string AWS_S3_BUCKET_BACKUPS = PrivateConfig.AWS_S3_BUCKET_BACKUPS;

        public static readonly int DAYS_TO_DELETE_BACKUP = 15;

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
