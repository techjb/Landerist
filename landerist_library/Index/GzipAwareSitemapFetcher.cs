using landerist_library.Configuration;
using Louw.SitemapParser;
using System.IO.Compression;
using System.Net;

namespace landerist_library.Index
{
    internal sealed class GzipAwareSitemapFetcher : ISitemapFetcher
    {
        private static readonly HttpClient HttpClient = CreateHttpClient(useProxy: false);
        private readonly bool UseProxy;

        public GzipAwareSitemapFetcher(bool useProxy = false)
        {
            UseProxy = useProxy;
        }

        public async Task<string> Fetch(Uri uri)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, uri);
            if (UseProxy)
            {
                using HttpClient httpClient = CreateHttpClient(useProxy: true);
                using HttpResponseMessage proxyResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                proxyResponse.EnsureSuccessStatusCode();
                return await ReadContentAsync(uri, proxyResponse).ConfigureAwait(false);
            }

            using HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            return await ReadContentAsync(uri, response).ConfigureAwait(false);
        }

        private static HttpClient CreateHttpClient(bool useProxy)
        {
            HttpClient client = useProxy
                ? new HttpClient(CreateProxyHandler())
                : new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT_ROBOTSTXT);

            return client;
        }

        private static HttpClientHandler CreateProxyHandler()
        {
            return new HttpClientHandler
            {
                UseProxy = true,
                Proxy = new WebProxy(PrivateConfig.PROXY_HOST, GetProxyPort())
                {
                    Credentials = new NetworkCredential(
                        PrivateConfig.PROXY_USERNAME,
                        PrivateConfig.PROXY_PASSWORD)
                }
            };
        }

        private static int GetProxyPort()
        {
            if (!PrivateConfig.PROXY_RANDOMIZE_STICKY_PORTS ||
                PrivateConfig.PROXY_STICKY_PORT_MIN > PrivateConfig.PROXY_STICKY_PORT_MAX)
            {
                return int.Parse(PrivateConfig.PROXY_PORT);
            }

            return Random.Shared.Next(
                PrivateConfig.PROXY_STICKY_PORT_MIN,
                PrivateConfig.PROXY_STICKY_PORT_MAX + 1);
        }

        private static async Task<string> ReadContentAsync(Uri uri, HttpResponseMessage response)
        {
            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (!IsGzipContent(uri, response))
            {
                using StreamReader reader = new(responseStream);
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            using GZipStream gzipStream = new(responseStream, CompressionMode.Decompress);
            using StreamReader gzipReader = new(gzipStream);
            return await gzipReader.ReadToEndAsync().ConfigureAwait(false);
        }

        private static bool IsGzipContent(Uri uri, HttpResponseMessage response)
        {
            if (uri.AbsolutePath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (response.Content.Headers.ContentEncoding.Any(encoding => string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return string.Equals(response.Content.Headers.ContentType?.MediaType, "application/x-gzip", StringComparison.OrdinalIgnoreCase)
                || string.Equals(response.Content.Headers.ContentType?.MediaType, "application/gzip", StringComparison.OrdinalIgnoreCase);
        }
    }
}
