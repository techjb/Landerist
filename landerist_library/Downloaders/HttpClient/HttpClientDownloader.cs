using landerist_library.Configuration;
using landerist_library.Websites;
using System.Net.Http.Headers;

namespace landerist_library.Downloaders.HttpClient
{
    public class HttpClientDownloader : IDownloader
    {
        public short? HttpStatusCode { get; set; } = null;
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;
        public string? Etag { get; set; } = null;
        public string? LastModified { get; set; } = null;

        private HttpResponseMessage? HttpResponseMessage;

        public void Download(Page page)
        {
            HttpStatusCode = null;
            Content = null;
            Screenshot = null;
            RedirectUrl = null;
            Etag = null;
            LastModified = null;

            GetAsync(page);
            if (HttpResponseMessage != null)
            {
                HttpStatusCode = (short)HttpResponseMessage.StatusCode;
                Etag = HttpResponseMessage.Headers.ETag?.ToString();
                LastModified = GetLastModified();
            }
            page.SetDownloadedData(this);
        }

        private string? GetLastModified()
        {
            if (HttpResponseMessage?.Content.Headers.TryGetValues("Last-Modified", out var lastModifiedValues) == true)
            {
                return lastModifiedValues.FirstOrDefault();
            }

            return null;
        }

        public async void GetAsync(Page page)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false,
                // trust certificate allways
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            using var httpClient = new System.Net.Http.HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            using HttpRequestMessage httpRequestMessage = page.Website.CreateHttpRequestMessage(HttpMethod.Get, page.Uri);
            SetAccepLanguage(httpRequestMessage, page.Website.LanguageCode);
            HttpResponseMessage = null;

            try
            {
                DateTime dateStart = DateTime.Now;
                HttpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                Timers.Timer.SaveTimerDownloadPage(page.Uri.ToString(), dateStart);
                SetRedirectUrl();
                Content = await HttpResponseMessage.Content.ReadAsStringAsync();
            }
            catch// (Exception exception)
            {
                //Logs.Log.WriteLogErrors("HttpClientDownloader GetAsync", page.Uri, exception);
            }
        }

        private static void SetAccepLanguage(HttpRequestMessage request, LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LanguageCode.es:
                    {
                        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("es-ES"));
                        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 0.9));
                    }
                    break;
            }
        }

        private void SetRedirectUrl()
        {
            if (HttpResponseMessage != null &&
                HttpResponseMessage.Headers.TryGetValues("Location", out var locations))
            {
                RedirectUrl = locations.FirstOrDefault();
            }
        }

        public string? GetRedirectUrl()
        {
            return RedirectUrl;
        }
    }
}
