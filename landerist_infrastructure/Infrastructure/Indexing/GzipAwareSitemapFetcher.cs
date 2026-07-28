using landerist_library.Infrastructure.Http;
using landerist_library.Websites;
using Louw.SitemapParser;
using System.IO.Compression;

namespace landerist_library.Infrastructure.Indexing
{
    internal sealed class GzipAwareSitemapFetcher : ISitemapFetcher
    {
        private readonly WebsiteHttpRequestProfile RequestProfile;
        private readonly bool UseProxy;
        private readonly TimeSpan Timeout;
        private readonly IHttpClientTransportFactory HttpClients;

        public GzipAwareSitemapFetcher(Website website, IHttpClientTransportFactory httpClients)
        {
            ArgumentNullException.ThrowIfNull(httpClients);
            RequestProfile = WebsiteHttpRequestProfile.From(website);
            HttpClients = httpClients;
            Timeout = TimeSpan.FromSeconds(website.Rules.HttpClientTimeoutSeconds);
            UseProxy = website.UseProxy;
        }

        public async Task<string> Fetch(Uri uri)
        {
            using HttpRequestMessage request = RequestProfile.CreateRequest(HttpMethod.Get, uri);

            if (UseProxy)
            {
                using HttpClient httpClient = HttpClients.Create(useProxy: true, Timeout);
                using HttpResponseMessage proxyResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                proxyResponse.EnsureSuccessStatusCode();
                return await ReadContentAsync(uri, proxyResponse).ConfigureAwait(false);
            }

            using HttpClient directClient = HttpClients.Create(useProxy: false, Timeout);
            using HttpResponseMessage response = await directClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            return await ReadContentAsync(uri, response).ConfigureAwait(false);
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
