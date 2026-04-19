using Louw.SitemapParser;
using System.IO.Compression;
using System.Net.Http;

namespace landerist_library.Index
{
    internal sealed class GzipAwareSitemapFetcher : ISitemapFetcher
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        public async Task<string> Fetch(Uri uri)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, uri);
            using HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            return await ReadContentAsync(uri, response).ConfigureAwait(false);
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(Configuration.Config.HTTPCLIENT_SECONDS_TIMEOUT),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Configuration.Config.USER_AGENT);

            return client;
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
