namespace landerist_library.Configuration
{
    public class Config
    {
        private static bool ConfigurationProduction = true;

        public static readonly bool TRAINING_MODE = false;

        public static readonly bool SKIP_PARSE_LISTINGS = true;

        private static readonly string VERSION_PRODUCCION = "1.00";

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

        private static readonly string EXPORT_DIRECTORY_LOCAL = PrivateConfig.EXPORT_DIRECTORY_LOCAL;

        private static readonly string EXPORT_DIRECTORY_PRODUCTION = PrivateConfig.EXPORT_DIRECTORY_PRODUCTION;

        public static readonly string USER_AGENT = "Mozilla/5.0 (compatible; AcmeInc/1.0)";


        public static bool IsConfigurationProduction()
        {
            return ConfigurationProduction;
        }

        public static void Init(bool configurationProduction)
        {
            ConfigurationProduction = configurationProduction;

            VERSION = ConfigurationProduction ?
                VERSION_PRODUCCION :
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
