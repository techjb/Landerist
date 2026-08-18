using landerist_library.Configuration;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Parsing;

namespace landerist_console;

internal static class LanderistRuntimeOptionsAdapter
{
    public static LanderistRuntimeOptions FromLegacyConfiguration()
    {
        Config.SetToProduction();
        LanderistSettings settings = LanderistSettings.Current;
        LanderistExecutionRole role = Config.IsLocalAIMachine()
            ? LanderistExecutionRole.LocalAi
            : Config.IsPrincipalMachine()
                ? LanderistExecutionRole.Principal
                : LanderistExecutionRole.Scraper;

        LanderistRuntimeOptions options = new(
            new DatabaseRuntimeOptions(
                Config.DATASOURCE ?? string.Empty,
                Config.DATABASE_USER,
                Config.DATABASE_PW,
                Config.DATABASE_NAME,
                Config.DATABASE_ENCRYPT,
                Config.DATABASE_TRUST_SERVER_CERTIFICATE),
            new ProxyRuntimeOptions(
                settings.GetString("PROXY_HOST"),
                settings.GetInt32("PROXY_PORT"),
                settings.GetBoolean("PROXY_RANDOMIZE_STICKY_PORTS"),
                settings.GetInt32("PROXY_STICKY_PORT_MIN"),
                settings.GetInt32("PROXY_STICKY_PORT_MAX"),
                settings.GetString("PROXY_USERNAME"),
                settings.GetString("PROXY_PASSWORD")),
            new BrowserRuntimeOptions(
                Config.HEADLESS_BROWSER,
                Config.IsConfigurationLocal(),
                checked(Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000),
                ProcessCleanupEnabled: Config.IsConfigurationProduction(),
                UseTaskKillFallback: Config.IsPrincipalMachine()),
            role)
        {
            Ai = CreateAiOptions(),
            Batch = CreateBatchOptions(),
            Scraping = new ScrapingRuntimeOptions(
                Config.NOT_LISTING_CACHE_ENABLED,
                Config.MAX_PAGES_PER_WEBSITE,
                Config.INDEXER_ENABLED,
                Config.MAX_PAGES_PER_HOST_PER_SCRAPE,
                Config.MAX_PAGES_PER_SCRAPE,
                Config.MIN_PAGES_PER_SCRAPE,
                Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
                Config.HTTPCLIENT_SECONDS_TIMEOUT),
            Integrations = new IntegrationRuntimeOptions(
                settings.GetString("SCRAPPINGBEE_APIKEY"),
                settings.GetString("AWS_ACESSKEYID"),
                settings.GetString("AWS_SECRETACCESSKEY"),
                settings.GetString("AWS_S3_DOWNLOADS_BUCKET"),
                settings.GetString("AWS_S3_WEBSITE_BUCKET"),
                settings.GetString("GOOLZOOM_API"),
                settings.GetString("GOOGLE_CLOUD_LANDERIST_API_KEY")),
            Execution = new ExecutionRuntimeOptions(
                Config.IsConfigurationLocal(),
                Config.IsConfigurationProduction(),
                Config.MACHINE_NAME,
                Config.LOGS_ENABLED,
                Config.LOGS_ERRORS_IN_CONSOLE,
                Config.LOGS_INFO_IN_CONSOLE,
                Config.LOCAL_AI_MAX_MODEL_LEN,
                Config.VERSION),
            Distribution = new DistributionOptions(
                Config.EXPORT_DIRECTORY ?? string.Empty,
                Config.LANDERIST_COM_TEMPLATES ?? string.Empty,
                Config.LANDERIST_COM_OUTPUT ?? string.Empty,
                settings.GetString("AWS_S3_DOWNLOADS_BUCKET"),
                settings.GetString("AWS_ACESSKEYID"),
                settings.GetString("AWS_SECRETACCESSKEY"),
                settings.GetString("AWS_CLOUDFRONT_DISTRIBUTION_ID_WEBSITE")),
            Backup = new DatabaseBackupOptions(
                Config.DATABASE_NAME,
                Config.BACKUPS_DIRECTORY ?? string.Empty,
                settings.GetString("AWS_S3_BACKUPS_BUCKET"),
                Config.DAYS_TO_DELETE_BACKUP),
            Administration = new AdministrationOptions(
                Config.DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS,
                Path.Combine(settings.GetString("INSERT_DIRECTORY"), "HostMainUri.csv"))
        };

        options.Validate();
        return options;
    }
    private static AiRuntimeOptions CreateAiOptions()
    {
        LanderistSettings settings = LanderistSettings.Current;
        return new AiRuntimeOptions(
            settings.GetString("OPENAI_API_KEY"),
            settings.GetString("GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL"),
            settings.GetString("GOOGLE_CLOUD_VERTEX_AI_PROJECTID"),
            settings.GetString("GOOGLE_CLOUD_VERTEX_AI_LOCATION"),
            settings.GetString("GOOGLE_CLOUD_VERTEX_AI_PUBLISHER"),
            Config.VERTEX_AI_MODEL_NAME_GEMINI_FLASH_LITE,
            Config.VERTEX_AI_MODEL_NAME_GEMINI_FLASH,
            Config.IsConfigurationLocal()
                ? settings.GetString("MACHINE_NAME_LANDERIST_03")
                : "localhost",
            Config.IsConfigurationLocal(),
            Config.LLM_PROVIDER);
    }

    private static BatchRuntimeOptions CreateBatchOptions()
    {
        int maxPages = Config.IsConfigurationLocal()
            ? Config.MAX_PAGES_PER_BATCH_LOCAL
            : Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => Config.MAX_PAGES_PER_BATCH_OPEN_AI,
                LLMProvider.VertexAI => Config.MAX_PAGES_PER_BATCH_VERTEX_AI,
                _ => throw new InvalidOperationException(
                    $"Batch upload is not supported for {Config.LLM_PROVIDER}.")
            };
        long maxFileSizeBytes = Config.LLM_PROVIDER switch
        {
            LLMProvider.OpenAI => Config.MAX_BATCH_FILE_SIZE_OPEN_AI * 1024L * 1024L,
            LLMProvider.VertexAI => Config.MAX_BATCH_FILE_SIZE_VERTEX_AI * 1024L * 1024L,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {Config.LLM_PROVIDER}.")
        };

        return new BatchRuntimeOptions(
            Config.BATCH_ENABLED,
            Config.BATCH_DIRECTORY ?? string.Empty,
            maxPages,
            Config.MIN_PAGES_PER_BATCH,
            maxFileSizeBytes,
            Config.PARALLELOPTIONS1INLOCAL.MaxDegreeOfParallelism,
            !Config.IsConfigurationLocal(),
            Math.Abs(Config.DAYS_TO_REMOVE_BATCH_FILES),
            LanderistSettings.Current.GetString("GOOGLE_CLOUD_BUCKET_NAME"));
    }
}
