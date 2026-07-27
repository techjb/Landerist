using System.Net;

namespace landerist_library.Infrastructure.Http;

public sealed class HttpClientTransportFactory
{
    private readonly HttpTransportOptions _options;
    private readonly Func<int, int, int> _nextPort;

    public HttpClientTransportFactory(
        HttpTransportOptions options,
        Func<int, int, int>? nextPort = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _nextPort = nextPort ?? Random.Shared.Next;
    }

    public HttpClient Create(
        bool useProxy,
        TimeSpan timeout,
        bool allowAutoRedirect = true)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                "HTTP timeout must be positive.");
        }

        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = allowAutoRedirect
        };

        if (useProxy)
        {
            handler.UseProxy = true;
            handler.Proxy = new WebProxy(_options.ProxyHost, GetProxyPort())
            {
                Credentials = new NetworkCredential(
                    _options.ProxyUsername,
                    _options.ProxyPassword)
            };
        }

        return new HttpClient(handler)
        {
            Timeout = timeout
        };
    }

    public int GetProxyPort()
    {
        if (!_options.RandomizeStickyPorts ||
            _options.StickyPortMin > _options.StickyPortMax)
        {
            return _options.ProxyPort;
        }

        return _nextPort(
            _options.StickyPortMin,
            checked(_options.StickyPortMax + 1));
    }
}
