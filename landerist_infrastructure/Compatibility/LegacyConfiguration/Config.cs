using landerist_library.Parsing;

namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        public static readonly string VERSION = "5.94";

        public static readonly bool INDEXER_ENABLED = true;

        public static readonly bool MEDIA_PARSER_ENABLED = true;

        public static readonly int MAX_PAGES_PER_WEBSITE = 40000;

        public static readonly int MIN_PAGES_PER_SCRAPE = 10;

        public static readonly int MAX_PAGES_PER_SCRAPE = 2500;

        public static readonly int MAX_PAGES_PER_HOST_PER_SCRAPE = 2;

        public static readonly int MIN_LISTINGPARSERINPUT_LENGTH = 50;

        public static readonly int MAX_LISTINGPARSERINPUT_LENGTH = 100000;

        public static readonly int MAX_PAGETYPE_COUNTER = 1000;

        public static readonly bool LOGS_ENABLED = true;

        public static readonly bool LOGS_INFO_IN_CONSOLE = true;

        public static bool LOGS_ERRORS_IN_CONSOLE = false;
        public static bool TIMERS_ENABLED { get; set; }
        
        public static readonly string USER_AGENT_BROWSER = "Mozilla/5.0 (compatible; AcmeInc/1.0)";        

        public static readonly int HTTPCLIENT_SECONDS_TIMEOUT = 10;

        public static readonly int MAX_CRAW_DELAY_SECONDS = 300;

        public static readonly double MINIMUM_PERCENTAGE_TO_BE_SIMILAR_PAGE = 0.85;

        public static string DATABASE_NAME => AppConfig.DATABASE_NAME;

        public static string DATABASE_USER => AppConfig.DATABASE_USER;

        public static string DATABASE_PW => AppConfig.DATABASE_PW;

        public static string? DATASOURCE { get; set; }

        public static bool DATABASE_ENCRYPT { get; private set; }

        public static bool DATABASE_TRUST_SERVER_CERTIFICATE { get; private set; }

        public static string? EXPORT_DIRECTORY { get; set; }
        public static string? LANDERIST_COM_OUTPUT { get; set; }
        public static string? LANDERIST_COM_TEMPLATES { get; set; }
        public static string? BACKUPS_DIRECTORY { get; set; }
        public static string? SCREENSHOTS_DIRECTORY { get; set; }
        public static bool SAVE_SCREENSHOT_FILE { get; set; }

public const int MAX_SCREENSHOT_SIZE = 5 * 1024 * 1024; // 5 MB                

        public const int MAX_SCREENSHOT_PIXELS_SIDE = 8000;
        public static string? CHROME_EXTENSIONS_DIRECTORY { get; set; }

        public static readonly int DAYS_TO_DELETE_BACKUP = 60;

        public const int MAX_YEARS_SINCE_PUBLISHED_LISTING = 5;

        public const int MIN_CONSTRUCTION_YEAR = 1800;

        public const int MAX_CONSTRUCTION_YEARS_FROM_NOW = 5;

        public const int MIN_PROPERTY_SIZE = 1;

        public const int MAX_PROPERTY_SIZE = 100000;

        public const int MIN_LAND_SIZE = 10;

        public const int MAX_LAND_SIZE = 10000000;

        public const int MIN_FLOORS = 0;

        public const int MAX_FLOORS = 500;

        public const int MIN_BEDROOMS = 0;

        public const int MAX_BEDROOMS = 50;

        public const int MIN_BATHROOMS = 0;

        public const int MAX_BATHROOMS = 20;

        public const int MIN_PARKINGS = 0;

        public const int MAX_PARKINGS = 10000;

        public const int DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS = 180;
        public static LLMProvider LLM_PROVIDER { get; set; }
        public static string? BATCH_DIRECTORY { get; set; }
        public static bool BATCH_ENABLED { get; set; }

        public const int MAX_PAGES_PER_BATCH_LOCAL = 10;

        public const int MAX_PAGES_PER_BATCH_OPEN_AI = 1000;

        public const int MAX_PAGES_PER_BATCH_VERTEX_AI = 10000;
        public static int MIN_PAGES_PER_BATCH { get; set; }

        public const int MAX_BATCH_FILE_SIZE_OPEN_AI = 90;

        public const int MAX_BATCH_FILE_SIZE_VERTEX_AI = 200;

        public static int DAYS_TO_REMOVE_BATCH_FILES { get; set; }
        public static int MAX_DEGREE_OF_PARALLELISM_SCRAPER { get; set; }

        public static ParallelOptions PARALLELOPTIONS1INLOCAL = new() { };

        public readonly static string MACHINE_NAME = Environment.MachineName;

        public static readonly string VERTEX_AI_MODEL_NAME_GEMINI_PRO = "gemini-2.5-pro";

        public static readonly string VERTEX_AI_MODEL_NAME_GEMINI_FLASH = "gemini-2.5-flash";

        public static readonly string VERTEX_AI_MODEL_NAME_GEMINI_FLASH_LITE = "gemini-2.5-flash-lite";
       

        public const int LOCAL_AI_MAX_MODEL_LEN = 30000; 

        public const bool NOT_LISTING_CACHE_ENABLED = false;

        public static bool HEADLESS_BROWSER { get; set; }


        public static bool IsConfigurationProduction()
        {
            return ConfigurationProduction;
        }

        public static bool IsConfigurationLocal()
        {
            return !ConfigurationProduction;
        }

        public static bool IsPrincipalMachine()
        {
            return MACHINE_NAME.Equals(AppConfig.MACHINE_NAME_LANDERIST_01);
        }

        public static bool IsLocalAIMachine()
        {
            return MACHINE_NAME.Equals(AppConfig.MACHINE_NAME_LANDERIST_03);
        }

        public static void SetToProduction()
        {
            Init(true);
        }

        public static void SetToLocal()
        {
            Init(false);
        }

        public static void SetToTest()
        {
            SetToLocal();

            string productionDataSource = AppConfig.DATASOURCE_PRODUCTION;
            if (!string.IsNullOrWhiteSpace(productionDataSource)
                && string.Equals(
                    DATASOURCE?.Trim(),
                    productionDataSource.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Test configuration points to the production datasource. " +
                    "Set DATASOURCE_LOCAL to a separate SQL Server instance.");
            }
        }

        private static void Init(bool configurationProduction)
        {
            ConfigurationProduction = configurationProduction;

            ValidateDatabaseConfiguration(configurationProduction);

            if (!string.IsNullOrWhiteSpace(AppConfig.NEWTONSOFT_LICENSE_KEY))
            {
                Newtonsoft.Json.Schema.License.RegisterLicense(AppConfig.NEWTONSOFT_LICENSE_KEY);
            }

            InitDatabase(configurationProduction);

            EXPORT_DIRECTORY = ConfigurationProduction ?
                AppConfig.EXPORT_DIRECTORY_PRODUCTION :
                AppConfig.EXPORT_DIRECTORY_LOCAL;

            LANDERIST_COM_OUTPUT = ConfigurationProduction ?
                AppConfig.LANDERIST_COM_OUTPUT_PRODUCTION :
                AppConfig.LANDERIST_COM_OUTPUT_LOCAL;

            LANDERIST_COM_TEMPLATES = configurationProduction ?
                AppConfig.LANDERIST_COM_TEMPLATES_PRODUCTION :
                AppConfig.LANDERIST_COM_TEMPLATES_LOCAL;

            BACKUPS_DIRECTORY = ConfigurationProduction ?
                AppConfig.BACKUPS_DIRECTORY_PRODUCTION :
                AppConfig.BACKUPS_DIRECTORY_LOCAL;

            SCREENSHOTS_DIRECTORY = ConfigurationProduction ?
                AppConfig.SCREENSHOTS_DIRECTORY_PRODUCTION :
                AppConfig.SCREENSHOTS_DIRECTORY_LOCAL;

            SAVE_SCREENSHOT_FILE = !ConfigurationProduction;

            CHROME_EXTENSIONS_DIRECTORY = ConfigurationProduction ?
                AppConfig.CHROME_EXTENSIONS_DIRECTORY_PRODUCTION :
                AppConfig.CHROME_EXTENSIONS_DIRECTORY_LOCAL;


            TIMERS_ENABLED = !ConfigurationProduction;

            LLM_PROVIDER = ConfigurationProduction ?
                LLMProvider.VertexAI :
                LLMProvider.LocalAI;


            BATCH_ENABLED = ConfigurationProduction;

            MIN_PAGES_PER_BATCH = ConfigurationProduction ? 500 : 1;

            BATCH_DIRECTORY = ConfigurationProduction ?
                AppConfig.BATCH_DIRECTORY_PRODUCTION :
                AppConfig.BATCH_DIRECTORY_LOCAL;

            DAYS_TO_REMOVE_BATCH_FILES = -30;

            MAX_DEGREE_OF_PARALLELISM_SCRAPER = ConfigurationProduction ?
                //Environment.ProcessorCount * 70 / 100 : // % of the processors
                30 :
                1;                

            //MAX_DEGREE_OF_PARALLELISM_SCRAPER = 5;

            PARALLELOPTIONS1INLOCAL = ConfigurationProduction ?
                new() :
                new ParallelOptions() { MaxDegreeOfParallelism = 1 };


            HEADLESS_BROWSER = ConfigurationProduction;
            //HEADLESS_BROWSER = false;
        }

        private static void InitDatabase(bool configurationProduction)
        {
            DATASOURCE = configurationProduction ?
                AppConfig.DATASOURCE_PRODUCTION :
                AppConfig.DATASOURCE_LOCAL;

            DATABASE_ENCRYPT = configurationProduction ?
                AppConfig.DATABASE_ENCRYPT_PRODUCTION :
                AppConfig.DATABASE_ENCRYPT_LOCAL;

            DATABASE_TRUST_SERVER_CERTIFICATE = configurationProduction ?
                AppConfig.DATABASE_TRUST_SERVER_CERTIFICATE_PRODUCTION :
                AppConfig.DATABASE_TRUST_SERVER_CERTIFICATE_LOCAL;
        }

        private static void ValidateDatabaseConfiguration(bool configurationProduction)
        {
            LanderistSettings.Current.Validate(
                "DATABASE_NAME",
                "DATABASE_USER",
                "DATABASE_PW",
                configurationProduction ? "DATASOURCE_PRODUCTION" : "DATASOURCE_LOCAL");
        }

        public static void SetLLMProviderLocalAI()
        {
            LLM_PROVIDER = LLMProvider.LocalAI;
        }

        public static void EnableLogsErrorsInConsole()
        {
            LOGS_ERRORS_IN_CONSOLE = true;
        }
    }
}
