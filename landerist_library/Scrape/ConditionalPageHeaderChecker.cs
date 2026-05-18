using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace landerist_library.Scrape
{
    internal sealed class ConditionalPageHeaderChecker
    {
        private readonly bool UseProxy;

        public ConditionalPageHeaderChecker(bool useProxy)
        {
            UseProxy = useProxy;
        }

        public ConditionalHeaderCheckResult Check(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            try
            {
                return CheckAsync(page).GetAwaiter().GetResult();
            }
            catch
            {
                return new ConditionalHeaderCheckResult();
            }
        }

        private async Task<ConditionalHeaderCheckResult> CheckAsync(Page page)
        {
            using var httpClient = CreateHttpClient();
            using var request = CreateRequest(page);
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            var result = new ConditionalHeaderCheckResult
            {
                HttpStatusCode = (short)response.StatusCode,
                RedirectUrl = GetRedirectUrl(page.Uri, response),
                Etag = GetEtag(response),
                LastModified = GetLastModified(response)
            };

            return result with
            {
                NotModified = IsNotModified(page, response, result)
            };
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = UseProxy
                ? new HttpClient(CreateProxyHandler())
                : new HttpClient(CreateHandler());

            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            return httpClient;
        }

        private static HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
        }

        private static HttpClientHandler CreateProxyHandler()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false,
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
                return int.Parse(PrivateConfig.PROXY_PORT, CultureInfo.InvariantCulture);
            }

            return Random.Shared.Next(
                PrivateConfig.PROXY_STICKY_PORT_MIN,
                PrivateConfig.PROXY_STICKY_PORT_MAX + 1);
        }

        private static HttpRequestMessage CreateRequest(Page page)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, page.Uri);
            SetAcceptLanguage(request, page.Website.LanguageCode);

            if (!string.IsNullOrWhiteSpace(page.Etag))
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", page.Etag.Trim());
            }

            if (!string.IsNullOrWhiteSpace(page.LastModified))
            {
                request.Headers.TryAddWithoutValidation("If-Modified-Since", page.LastModified.Trim());
            }

            return request;
        }

        private static void SetAcceptLanguage(HttpRequestMessage request, LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LanguageCode.es:
                    request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("es-ES"));
                    request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 0.9));
                    break;
            }
        }

        private static bool IsNotModified(
            Page page,
            HttpResponseMessage response,
            ConditionalHeaderCheckResult result)
        {
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return true;
            }

            if (response.StatusCode != HttpStatusCode.OK || !string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                return false;
            }

            return DownloadedHeadersHaveNotChanged(page, result);
        }

        private static bool DownloadedHeadersHaveNotChanged(Page page, ConditionalHeaderCheckResult result)
        {
            var previousEtag = NormalizeHeaderValue(page.Etag);
            var downloadedEtag = NormalizeHeaderValue(result.Etag);
            var previousLastModified = NormalizeHeaderValue(page.LastModified);
            var downloadedLastModified = NormalizeHeaderValue(result.LastModified);

            var hasComparableEtag = !string.IsNullOrEmpty(previousEtag) && !string.IsNullOrEmpty(downloadedEtag);
            if (hasComparableEtag)
            {
                return string.Equals(previousEtag, downloadedEtag, StringComparison.Ordinal);
            }

            return !string.IsNullOrEmpty(previousLastModified) &&
                !string.IsNullOrEmpty(downloadedLastModified) &&
                HeaderDateEquals(previousLastModified, downloadedLastModified);
        }

        private static bool HeaderDateEquals(string previousLastModified, string downloadedLastModified)
        {
            if (DateTimeOffset.TryParse(previousLastModified, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var previousDate) &&
                DateTimeOffset.TryParse(downloadedLastModified, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var downloadedDate))
            {
                return previousDate.ToUniversalTime().Equals(downloadedDate.ToUniversalTime());
            }

            return string.Equals(previousLastModified, downloadedLastModified, StringComparison.Ordinal);
        }

        private static string? GetRedirectUrl(Uri requestUri, HttpResponseMessage response)
        {
            var location = response.Headers.Location;
            if (location is null)
            {
                return null;
            }

            if (!location.IsAbsoluteUri)
            {
                location = new Uri(requestUri, location);
            }

            return requestUri.Equals(location) ? null : location.ToString();
        }

        private static string? GetEtag(HttpResponseMessage response)
        {
            return NormalizeHeaderValue(response.Headers.ETag?.ToString());
        }

        private static string? GetLastModified(HttpResponseMessage response)
        {
            var lastModified = response.Content.Headers.LastModified;
            if (lastModified.HasValue)
            {
                return lastModified.Value.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
            }

            if (response.Content.Headers.TryGetValues("Last-Modified", out var contentValues))
            {
                return NormalizeHeaderValue(contentValues.FirstOrDefault());
            }

            return response.Headers.TryGetValues("Last-Modified", out var headerValues)
                ? NormalizeHeaderValue(headerValues.FirstOrDefault())
                : null;
        }

        private static string? NormalizeHeaderValue(string? headerValue)
        {
            return string.IsNullOrWhiteSpace(headerValue) ? null : headerValue.Trim();
        }
    }
}
