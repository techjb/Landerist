using System.Globalization;
using System.Text.Json;

namespace landerist_library.Configuration;

public sealed class LanderistSettings
{
    private const string EnvironmentPrefix = "LANDERIST__";
    private readonly Dictionary<string, string> _values;
    private LanderistSettings(Dictionary<string, string> values) => _values = values;
    public static LanderistSettings Current { get; } = Load();
    public string this[string key] => GetString(key);
    public string GetString(string key, string defaultValue = "") => _values.TryGetValue(key, out var value) ? value : defaultValue;
    public int GetInt32(string key, int defaultValue = 0) => int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    public bool GetBoolean(string key, bool defaultValue = false) => bool.TryParse(GetString(key), out var value) ? value : defaultValue;

    public void Validate(params string[] requiredKeys)
    {
        var missingKeys = requiredKeys.Where(key => string.IsNullOrWhiteSpace(GetString(key))).ToArray();
        if (missingKeys.Length > 0)
            throw new InvalidOperationException($"Missing required Landerist configuration: {string.Join(", ", missingKeys)}. Set the values in appsettings.Local.json or with LANDERIST__ environment variables.");
    }

    private static LanderistSettings Load()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LoadJsonFile(values, FindFile("appsettings.json"));
        LoadJsonFile(values, FindFile("appsettings.Local.json"));
        LoadEnvironmentVariables(values);
        return new LanderistSettings(values);
    }

    private static string? FindFile(string fileName)
    {
        var explicitConfigPath = Environment.GetEnvironmentVariable("LANDERIST_CONFIG_PATH");
        if (!string.IsNullOrWhiteSpace(explicitConfigPath))
        {
            var explicitFile = Directory.Exists(explicitConfigPath)
                ? Path.Combine(explicitConfigPath, fileName)
                : explicitConfigPath;

            if (File.Exists(explicitFile)
                && string.Equals(Path.GetFileName(explicitFile), fileName, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(explicitFile);
            }
        }

        foreach (var startDirectory in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static void LoadJsonFile(Dictionary<string, string> values, string? path)
    {
        if (path is null) return;
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("Landerist", out var section) || section.ValueKind != JsonValueKind.Object) return;
        foreach (var property in section.EnumerateObject())
            if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                values[property.Name] = property.Value.ToString();
    }

    private static void LoadEnvironmentVariables(Dictionary<string, string> values)
    {
        foreach (System.Collections.DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = variable.Key?.ToString();
            if (name?.StartsWith(EnvironmentPrefix, StringComparison.OrdinalIgnoreCase) == true)
                values[name[EnvironmentPrefix.Length..]] = variable.Value?.ToString() ?? string.Empty;
        }
    }

}

internal static class AppConfig
{
    private static LanderistSettings Settings => LanderistSettings.Current;
    public static string OPENAI_API_KEY => Settings["OPENAI_API_KEY"];
    public static string GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL => Settings["GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL"];
    public static string GOOGLE_CLOUD_VERTEX_AI_PROJECTID => Settings["GOOGLE_CLOUD_VERTEX_AI_PROJECTID"];
    public static string GOOGLE_CLOUD_VERTEX_AI_LOCATION => Settings["GOOGLE_CLOUD_VERTEX_AI_LOCATION"];
    public static string GOOGLE_CLOUD_VERTEX_AI_PUBLISHER => Settings["GOOGLE_CLOUD_VERTEX_AI_PUBLISHER"];
    public static string ANTHROPIC_API_KEY => Settings["ANTHROPIC_API_KEY"];
    public static string GEMINI_API_KEY => Settings["GEMINI_API_KEY"];
    public static string MACHINE_NAME_LANDERIST_01 => Settings["MACHINE_NAME_LANDERIST_01"];
    public static string MACHINE_NAME_LANDERIST_03 => Settings["MACHINE_NAME_LANDERIST_03"];
    public static string DATABASE_NAME => Settings["DATABASE_NAME"];
    public static string DATABASE_USER => Settings["DATABASE_USER"];
    public static string DATABASE_PW => Settings["DATABASE_PW"];
    public static string DATASOURCE_LOCAL => Settings["DATASOURCE_LOCAL"];
    public static string DATASOURCE_PRODUCTION => Settings["DATASOURCE_PRODUCTION"];
    public static bool DATABASE_ENCRYPT_LOCAL => Settings.GetBoolean("DATABASE_ENCRYPT_LOCAL", false);
    public static bool DATABASE_ENCRYPT_PRODUCTION => Settings.GetBoolean("DATABASE_ENCRYPT_PRODUCTION", true);
    public static bool DATABASE_TRUST_SERVER_CERTIFICATE_LOCAL => Settings.GetBoolean("DATABASE_TRUST_SERVER_CERTIFICATE_LOCAL", true);
    public static bool DATABASE_TRUST_SERVER_CERTIFICATE_PRODUCTION => Settings.GetBoolean("DATABASE_TRUST_SERVER_CERTIFICATE_PRODUCTION", true);
    public static string INSERT_DIRECTORY => Settings["INSERT_DIRECTORY"];
    public static string EXPORT_DIRECTORY_LOCAL => Settings["EXPORT_DIRECTORY_LOCAL"];
    public static string EXPORT_DIRECTORY_PRODUCTION => Settings["EXPORT_DIRECTORY_PRODUCTION"];
    public static string CLASSIFFIER_DIRECTORY => Settings["CLASSIFFIER_DIRECTORY"];
    public static string DELIMITATIONS_DIRECTORY => Settings["DELIMITATIONS_DIRECTORY"];
    public static string BACKUPS_DIRECTORY_LOCAL => Settings["BACKUPS_DIRECTORY_LOCAL"];
    public static string BACKUPS_DIRECTORY_PRODUCTION => Settings["BACKUPS_DIRECTORY_PRODUCTION"];
    public static string SCREENSHOTS_DIRECTORY_LOCAL => Settings["SCREENSHOTS_DIRECTORY_LOCAL"];
    public static string SCREENSHOTS_DIRECTORY_PRODUCTION => Settings["SCREENSHOTS_DIRECTORY_PRODUCTION"];
    public static string CHROME_EXTENSIONS_DIRECTORY_LOCAL => Settings["CHROME_EXTENSIONS_DIRECTORY_LOCAL"];
    public static string CHROME_EXTENSIONS_DIRECTORY_PRODUCTION => Settings["CHROME_EXTENSIONS_DIRECTORY_PRODUCTION"];
    public static string BATCH_DIRECTORY_LOCAL => Settings["BATCH_DIRECTORY_LOCAL"];
    public static string TRAININGDATA_DIRECTORY_LOCAL => Settings["TRAININGDATA_DIRECTORY_LOCAL"];
    public static string BATCH_DIRECTORY_PRODUCTION => Settings["BATCH_DIRECTORY_PRODUCTION"];
    public static string LANDERIST_COM_TEMPLATES_LOCAL => Settings["LANDERIST_COM_TEMPLATES_LOCAL"];
    public static string LANDERIST_COM_TEMPLATES_PRODUCTION => Settings["LANDERIST_COM_TEMPLATES_PRODUCTION"];
    public static string LANDERIST_COM_OUTPUT_LOCAL => Settings["LANDERIST_COM_OUTPUT_LOCAL"];
    public static string LANDERIST_COM_OUTPUT_PRODUCTION => Settings["LANDERIST_COM_OUTPUT_PRODUCTION"];
    public static string GOOGLE_CLOUD_LANDERIST_API_KEY => Settings["GOOGLE_CLOUD_LANDERIST_API_KEY"];
    public static string GOOGLE_SEARCH_ENGINE_ID => Settings["GOOGLE_SEARCH_ENGINE_ID"];
    public static string GOOLZOOM_API => Settings["GOOLZOOM_API"];
    public static string AWS_ACESSKEYID => Settings["AWS_ACESSKEYID"];
    public static string AWS_SECRETACCESSKEY => Settings["AWS_SECRETACCESSKEY"];
    public static string AWS_S3_DOWNLOADS_BUCKET => Settings["AWS_S3_DOWNLOADS_BUCKET"];
    public static string AWS_S3_BACKUPS_BUCKET => Settings["AWS_S3_BACKUPS_BUCKET"];
    public static string AWS_S3_WEBSITE_BUCKET => Settings["AWS_S3_WEBSITE_BUCKET"];
    public static string AWS_CLOUDFRONT_DISTRIBUTION_ID_WEBSITE => Settings["AWS_CLOUDFRONT_DISTRIBUTION_ID_WEBSITE"];
    public static string IDAGENCIES_URL => Settings["IDAGENCIES_URL"];
    public static string SCRAPPINGBEE_APIKEY => Settings["SCRAPPINGBEE_APIKEY"];
    public static string FT_AGENCIES_URL => Settings["FT_AGENCIES_URL"];
    public static string NEWTONSOFT_LICENSE_KEY => Settings["NEWTONSOFT_LICENSE_KEY"];
    public static string GOOGLE_CLOUD_BUCKET_NAME => Settings["GOOGLE_CLOUD_BUCKET_NAME"];
    public static string PROXY_HOST => Settings["PROXY_HOST"];
    public static string PROXY_PORT => Settings["PROXY_PORT"];
    public static bool PROXY_RANDOMIZE_STICKY_PORTS => Settings.GetBoolean("PROXY_RANDOMIZE_STICKY_PORTS");
    public static int PROXY_STICKY_PORT_MIN => Settings.GetInt32("PROXY_STICKY_PORT_MIN");
    public static int PROXY_STICKY_PORT_MAX => Settings.GetInt32("PROXY_STICKY_PORT_MAX");
    public static string PROXY_USERNAME => Settings["PROXY_USERNAME"];
    public static string PROXY_PASSWORD => Settings["PROXY_PASSWORD"];
}
