using System.Collections.ObjectModel;
using System.Text.Json;

namespace landerist_library.Websites;

public sealed class WebsiteHttpRequestProfile
{
    public string UserAgent { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public WebsiteHttpRequestProfile(
        string userAgent,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);
        UserAgent = userAgent.Trim();

        Dictionary<string, string> normalizedHeaders =
            new(StringComparer.OrdinalIgnoreCase);
        if (headers is not null)
        {
            foreach ((string name, string value) in headers)
            {
                AddHeader(normalizedHeaders, name, value);
            }
        }

        Headers = new ReadOnlyDictionary<string, string>(normalizedHeaders);
    }

    public static WebsiteHttpRequestProfile From(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return new WebsiteHttpRequestProfile(
            website.BrowserUserAgent,
            ParseHeaders(website.HttpRequestHeaders));
    }

    public HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(uri);

        HttpRequestMessage request = new(method, uri);
        request.Headers.UserAgent.ParseAdd(UserAgent);
        ApplyHeaders(request);
        return request;
    }

    public void ApplyHeaders(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        foreach ((string name, string value) in Headers)
        {
            if (!string.Equals(name, "User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }
    }

    private static IReadOnlyDictionary<string, string> ParseHeaders(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Dictionary<string, string>();
        }

        string trimmed = value.Trim();
        if (TryParseJsonHeaders(trimmed, out Dictionary<string, string> jsonHeaders))
        {
            return jsonHeaders;
        }

        return ParseLineHeaders(trimmed);
    }

    private static bool TryParseJsonHeaders(
        string value,
        out Dictionary<string, string> headers)
    {
        headers = new(StringComparer.OrdinalIgnoreCase);
        string json = value;
        if (!json.StartsWith('{') && json.StartsWith('"') && json.Contains(':'))
        {
            json = "{" + json + "}";
        }

        if (!json.StartsWith('{'))
        {
            return false;
        }

        try
        {
            Dictionary<string, string?>? parsed =
                JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            if (parsed is null)
            {
                return false;
            }

            foreach ((string name, string? headerValue) in parsed)
            {
                AddHeader(headers, name, headerValue);
            }

            return true;
        }
        catch (JsonException)
        {
            headers.Clear();
            return false;
        }
    }

    private static Dictionary<string, string> ParseLineHeaders(string value)
    {
        Dictionary<string, string> headers =
            new(StringComparer.OrdinalIgnoreCase);
        string[] lines = value.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        foreach (string line in lines)
        {
            int separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            string name = line[..separator].Trim().Trim('"');
            string headerValue = line[(separator + 1)..]
                .Trim()
                .TrimEnd(',')
                .Trim()
                .Trim('"');
            AddHeader(headers, name, headerValue);
        }

        return headers;
    }

    private static void AddHeader(
        IDictionary<string, string> headers,
        string? name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(name) &&
            !string.IsNullOrWhiteSpace(value))
        {
            headers[name.Trim()] = value.Trim();
        }
    }
}
