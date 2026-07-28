namespace landerist_library.Websites;

public static class WebsiteUrlRules
{
    private static readonly HashSet<string> WebPageExtensions =
    [
        ".htm",
        ".html",
        ".xhtml",
        ".asp",
        ".aspx",
        ".php",
        ".jsp",
        ".cshtml",
        ".vbhtml",
        ".razor"
    ];

    public static Uri Normalize(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        return new UriBuilder(uri) { Query = NormalizeQuery(uri.Query) }.Uri;
    }

    public static bool IsWebPage(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        string extension = Path.GetExtension(uri.AbsolutePath);
        return string.IsNullOrEmpty(extension) || WebPageExtensions.Contains(extension);
    }

    private static string NormalizeQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return string.Empty;
        }

        query = query.TrimStart('?');
        if (query.Length == 0)
        {
            return string.Empty;
        }

        List<string> orderedKeys = [];
        Dictionary<string, string?> keyedParameters = [];
        HashSet<string> flagParameters = [];
        foreach (string parameter in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            int separatorIndex = parameter.IndexOf('=');
            if (separatorIndex < 0)
            {
                if (parameter.Length > 0)
                {
                    flagParameters.Add(parameter);
                }
                continue;
            }

            string key = parameter[..separatorIndex];
            string value = parameter[(separatorIndex + 1)..];
            if (!keyedParameters.ContainsKey(key))
            {
                orderedKeys.Add(key);
            }
            keyedParameters[key] = value;
        }

        List<string> parts = orderedKeys
            .Select(key => $"{key}={keyedParameters[key]}")
            .ToList();
        parts.AddRange(flagParameters);
        return string.Join("&", parts);
    }
}