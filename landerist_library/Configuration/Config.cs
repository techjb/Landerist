using landerist_library.Parse.Listing;
using PuppeteerSharp;

namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        public static readonly string VERSION = "3.41";

        public static readonly bool INDEXER_ENABLED = true;

        public static readonly bool MEDIA_PARSER_ENABLED = false;

        public static readonly bool WORDS_ENABLED = false;

        public static readonly int MAX_PAGES_PER_WEBSITE = 500;

        public static readonly int MIN_PAGES_PER_SCRAPE = 100;

        public static readonly int MIN_PAGES_TO_FINALIZE_SCRAPE = 40;

        public static readonly int MAX_PAGES_PER_SCRAPE = 500;

        public static readonly int MAX_PAGES_PER_HOSTS_PER_SCRAPE = 10;

        public static readonly int MAX_SITEMAPS_PER_WEBSITE = 50;

        public static readonly int DAYS_TO_UPDATE_ROBOTS_TXT = 3;

        public static readonly int DAYS_TO_UPDATE_SITEMAP = 3;

        public static readonly int DAYS_TO_UPDATE_IP_ADDRESS = 3;

        public static readonly int MIN_RESPONSEBODYTEXT_LENGTH = 50;

        public static readonly int MAX_RESPONSEBODYTEXT_LENGTH = 100000;

        public static readonly int MAX_PAGETYPE_COUNTER = 1000;

        public static readonly int DEFAULT_DAYS_NEXT_UPDATE = 5;

        public static readonly bool LOGS_ENABLED = true;

        public static readonly bool LOGS_IN_CONSOLE = true;
        public static bool TIMERS_ENABLED { get; set; }

        public static readonly string USER_AGENT = "Mozilla/5.0 (compatible; AcmeInc/1.0)";

        public static readonly int HTTPCLIENT_SECONDS_TIMEOUT = 10;

        public static readonly int MAX_CRAW_DELAY_SECONDS = 60 * 60;

        public static readonly double MINIMUM_PERCENTAGE_TO_BE_SIMILAR_PAGE = 0.85;

        public const string DATABASE_NAME = PrivateConfig.DATABASE_NAME;

        public const string DATABASE_USER = PrivateConfig.DATABASE_USER;

        public const string DATABASE_PW = PrivateConfig.DATABASE_PW;

        private static readonly string CONNECTION_USER =
            "User ID=" + DATABASE_USER + ";" +
            "Password=" + DATABASE_PW + ";" +
            "Connect Timeout=100000;";

        public static string? DATASOURCE { get; set; }

        private static readonly string DATABASE_CONNECTION_LOCAL = CONNECTION_USER +
            "Data Source=" + PrivateConfig.DATASOURCE_LOCAL + ";" +
            "Initial Catalog=";

        private static readonly string DATABASE_CONNECTION_PRODUCTION = CONNECTION_USER +
            "Data Source=" + PrivateConfig.DATASOURCE_PRODUCTION + ";" +
            "Initial Catalog=";

        public static string? DATABASE_CONNECTION { get; set; }
        public static string? EXPORT_DIRECTORY { get; set; }
        public static string? LANDERIST_COM_OUTPUT { get; set; }
        public static string? LANDERIST_COM_TEMPLATES { get; set; }
        public static string? BACKUPS_DIRECTORY { get; set; }
        public static string? SCREENSHOTS_DIRECTORY { get; set; }
        public static bool TAKE_SCREENSHOT { get; set; }
        public static bool SAVE_SCREENSHOT_FILE { get; set; }

        public static readonly ScreenshotType SCREENSHOT_TYPE = ScreenshotType.Jpeg;

        public const int MAX_SCREENSHOT_SIZE = 5 * 1024 * 1024; // 5 MB                

        public const int MAX_SCREENSHOT_PIXELS_SIDE = 8000;
        public static string? CHROME_EXTENSIONS_DIRECTORY { get; set; }

        public static readonly int DAYS_TO_DELETE_BACKUP = 60;

        public const int MAX_YEARS_SINCE_PUBLISHED_LISTING = 5;

        public const int MIN_CONSTRUCTION_YEAR = 1980;

        public const int MAX_CONSTRUCTION_YEARS_FROM_NOW = 5;

        public const int MIN_PROPERTY_SIZE = 1;

        public const int MAX_PROPERTY_SIZE = 100000;

        public const int MIN_LAND_SIZE = 1;

        public const int MAX_LAND_SIZE = 10000000;

        public const int MIN_FLOORS = 0;

        public const int MAX_FLOORS = 500;

        public const int MIN_BEDROOMS = 0;

        public const int MAX_BEDROOMS = 50;

        public const int MIN_BATHROOMS = 0;

        public const int MAX_BATHROOMS = 20;

        public const int MIN_PARKINGS = 0;

        public const int MAX_PARKINGS = 10000;

        public const int DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS = 90;
        public static LLMProviders LLM_PROVIDER { get; set; }

        public static bool STRUCTURED_OUTPUT { get; set; }
        public static string? BATCH_DIRECTORY { get; set; }
        public static bool BATCH_ENABLED { get; set; }

        public const int MAX_PAGES_PER_BATCH = 10000;
        public static int MIN_PAGES_PER_BATCH { get; set; }

        public const int MAX_BATCH_FILE_SIZE_MB = 90;
        public static int MAX_DEGREE_OF_PARALLELISM_SCRAPER { get; set; }

        public static bool IsConfigurationProduction()
        {
            return ConfigurationProduction;
        }

        public static void SetToProduction()
        {
            Init(true);
        }

        public static void SetOnlyDatabaseToProduction()
        {
            SetToLocal();
            InitDatabase(true);
        }

        public static void SetToLocal()
        {
            Init(false);
        }

        private static void Init(bool configurationProduction)
        {
            ConfigurationProduction = configurationProduction;

            Newtonsoft.Json.Schema.License.RegisterLicense(PrivateConfig.NEWTONSOFT_LICENSE_KEY);

            InitDatabase(configurationProduction);

            EXPORT_DIRECTORY = ConfigurationProduction ?
                PrivateConfig.EXPORT_DIRECTORY_PRODUCTION :
                PrivateConfig.EXPORT_DIRECTORY_LOCAL;

            LANDERIST_COM_OUTPUT = ConfigurationProduction ?
                PrivateConfig.LANDERIST_COM_OUTPUT_PRODUCTION :
                PrivateConfig.LANDERIST_COM_OUTPUT_LOCAL;

            LANDERIST_COM_TEMPLATES = configurationProduction ?
                PrivateConfig.LANDERIST_COM_TEMPLATES_PRODUCTION :
                PrivateConfig.LANDERIST_COM_TEMPLATES_LOCAL;

            BACKUPS_DIRECTORY = ConfigurationProduction ?
                PrivateConfig.BACKUPS_DIRECTORY_PRODUCTION :
                PrivateConfig.BACKUPS_DIRECTORY_LOCAL;

            SCREENSHOTS_DIRECTORY = ConfigurationProduction ?
                PrivateConfig.SCREENSHOTS_DIRECTORY_PRODUCTION :
                PrivateConfig.SCREENSHOTS_DIRECTORY_LOCAL;

            TAKE_SCREENSHOT = false;

            SAVE_SCREENSHOT_FILE = !ConfigurationProduction;

            CHROME_EXTENSIONS_DIRECTORY = ConfigurationProduction ?
                PrivateConfig.CHROME_EXTENSIONS_DIRECTORY_PRODUCTION :
                PrivateConfig.CHROME_EXTENSIONS_DIRECTORY_LOCAL;


            TIMERS_ENABLED = !ConfigurationProduction;

            LLM_PROVIDER = !ConfigurationProduction ?
                LLMProviders.OpenAI :
                LLMProviders.OpenAI;

            STRUCTURED_OUTPUT = true;

            BATCH_ENABLED = LLM_PROVIDER.Equals(LLMProviders.OpenAI) && ConfigurationProduction;

            MIN_PAGES_PER_BATCH = ConfigurationProduction ? 300 : 1;

            BATCH_DIRECTORY = ConfigurationProduction ?
                PrivateConfig.BATCH_DIRECTORY_PRODUCTION :
                PrivateConfig.BATCH_DIRECTORY_LOCAL;

            MAX_DEGREE_OF_PARALLELISM_SCRAPER = ConfigurationProduction ?
                15 : 1;
        }

        private static void InitDatabase(bool configurationProduction)
        {
            DATASOURCE = configurationProduction ?
                PrivateConfig.DATASOURCE_PRODUCTION :
                PrivateConfig.DATASOURCE_LOCAL;

            DATABASE_CONNECTION = configurationProduction ?
                DATABASE_CONNECTION_PRODUCTION :
                DATABASE_CONNECTION_LOCAL;
        }
    }
}
