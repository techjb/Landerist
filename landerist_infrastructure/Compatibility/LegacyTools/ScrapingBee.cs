using landerist_library.Websites;

namespace landerist_library.Tools;

public static class ScrapingBee
{
    private static string? _apiKey;
    private static IHttpClientTransportFactory? _httpClients;

    public static void Configure(
        string apiKey,
        IHttpClientTransportFactory httpClients)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(httpClients);

        _apiKey = apiKey;
        _httpClients = httpClients;
    }

    public static string DownloadString(string url, bool allowAutoRedirect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        string apiKey = _apiKey
            ?? throw new InvalidOperationException("ScrapingBee is not configured.");
        IHttpClientTransportFactory httpClients = _httpClients
            ?? throw new InvalidOperationException("ScrapingBee is not configured.");
        string apiEndPoint = GetApiEndPointUrl(url, apiKey);

        using HttpClient httpClient = httpClients.Create(
            useProxy: false,
            timeout: TimeSpan.FromSeconds(100),
            allowAutoRedirect: allowAutoRedirect);
        using HttpResponseMessage response = httpClient
            .GetAsync(apiEndPoint)
            .GetAwaiter()
            .GetResult();
        return response.Content
            .ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();
    }

    private static string GetApiEndPointUrl(string url, string apiKey) =>
        "https://app.scrapingbee.com/api/v1/?" +
        "api_key=" + Uri.EscapeDataString(apiKey) +
        "&url=" + Uri.EscapeDataString(url) +
        "&render_js=false" +
        "&premium_proxy=true";
}