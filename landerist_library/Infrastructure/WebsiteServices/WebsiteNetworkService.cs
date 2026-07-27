using landerist_library.Application.Websites;
using landerist_library.Websites;
using System.Net;
using System.Text;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteNetworkService : IWebsiteNetworkService
{
    private const int MaxRedirects = 10;
    private readonly WebsiteNetworkOptions _options;
    private readonly TimeProvider _timeProvider;

    public WebsiteNetworkService(
        WebsiteNetworkOptions options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _options = options;
        _timeProvider = timeProvider;
    }

    public bool RefreshMainUri(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);

        for (int iteration = 0; iteration < MaxRedirects; iteration++)
        {
            using HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using HttpClient httpClient = new(handler)
            {
                Timeout = TimeSpan.FromSeconds(website.Rules.HttpClientTimeoutSeconds)
            };

            try
            {
                using HttpRequestMessage request =
                    website.CreateHttpRequestMessage(HttpMethod.Head, website.MainUri);
                using HttpResponseMessage response =
                    httpClient.SendAsync(request).GetAwaiter().GetResult();
                Uri? location = response.Headers.Location;

                if (location is null)
                {
                    return true;
                }

                if (!location.IsAbsoluteUri)
                {
                    location = new Uri(website.MainUri, location);
                }

                if (location.Equals(website.MainUri))
                {
                    return true;
                }

                website.MainUri = location;
                website.Host = location.Host;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public bool RefreshRobotsTxt(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        website.RobotsTxtUpdated = _timeProvider.GetLocalNow().DateTime;
        Uri robotsTxtUrl = new(website.MainUri, "/robots.txt");

        try
        {
            using HttpClient httpClient = CreateRobotsTxtHttpClient(website);
            using HttpRequestMessage request =
                website.CreateHttpRequestMessage(HttpMethod.Get, robotsTxtUrl);
            using HttpResponseMessage response =
                httpClient.SendAsync(request).GetAwaiter().GetResult();
            website.RobotsTxt = null;
            website.ResetParsedRobots();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using StreamReader streamReader = new(
                    response.Content.ReadAsStreamAsync().GetAwaiter().GetResult(),
                    Encoding.Default);
                website.RobotsTxt = streamReader.ReadToEnd();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RefreshIpAddress(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        website.IpAddressUpdated = _timeProvider.GetLocalNow().DateTime;

        try
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(website.Host);
            website.IpAddress =
                ipAddresses.Length > 0
                    ? ipAddresses[0].ToString()
                    : null;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private HttpClient CreateRobotsTxtHttpClient(Website website)
    {
        if (!website.UseProxy)
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(website.Rules.HttpClientTimeoutSeconds)
            };
        }

        HttpClientHandler handler = new()
        {
            UseProxy = true,
            Proxy = new WebProxy(_options.ProxyHost, GetProxyPort())
            {
                Credentials = new NetworkCredential(
                    _options.ProxyUsername,
                    _options.ProxyPassword)
            }
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(website.Rules.HttpClientTimeoutSeconds)
        };
    }

    private int GetProxyPort()
    {
        if (!_options.RandomizeStickyPorts ||
            _options.StickyPortMin > _options.StickyPortMax)
        {
            return _options.ProxyPort;
        }

        return Random.Shared.Next(
            _options.StickyPortMin,
            _options.StickyPortMax + 1);
    }
}
