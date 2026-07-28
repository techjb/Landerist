using landerist_library.Configuration;
using landerist_library.Infrastructure.Runtime;

namespace landerist_console;

internal static class LanderistRuntimeOptionsAdapter
{
    public static LanderistRuntimeOptions FromLegacyConfiguration()
    {
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
            role);

        options.Validate();
        return options;
    }
}
