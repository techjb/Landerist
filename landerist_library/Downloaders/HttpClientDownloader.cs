using landerist_library.Configuration;
using landerist_library.Websites;
using System.Net.Http.Headers;

namespace landerist_library.Downloaders
{
    public class HttpClientDownloader: IDownloader
    {
        public short? HttpStatusCode { get; set; } = null;
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;

        private HttpResponseMessage? HttpResponseMessage;

        public void Download(Page page)
        {
            GetAsync(page.Website.LanguageCode, page.Uri);
            if (HttpResponseMessage != null)
            {
                HttpStatusCode = (short)HttpResponseMessage.StatusCode;
            }
            page.SetDownloadedData(this);
        }

        public async void GetAsync(LanguageCode languageCode, Uri uri)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false,
                // trust certificate allways
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);

            SetAccepLanguage(httpClient, languageCode);
            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
            HttpResponseMessage = null;

            try
            {
                DateTime dateStart = DateTime.Now;
                HttpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                Timers.Timer.SaveTimerDownloadPage(uri.ToString(), dateStart);
                SetRedirectUrl();
                Content = await HttpResponseMessage.Content.ReadAsStringAsync();
            }
            catch// (Exception exception)
            {
                //Logs.Log.WriteLogErrors("HttpClientDownloader GetAsync", page.Uri, exception);
            }
        }

        private static void SetAccepLanguage(HttpClient httpClient, LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LanguageCode.es:
                    {
                        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es-ES"));
                        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 0.9));
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
