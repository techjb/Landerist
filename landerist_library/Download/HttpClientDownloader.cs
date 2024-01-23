using landerist_library.Configuration;
using landerist_library.Websites;
using System.Net.Http.Headers;

namespace landerist_library.Download
{
    public class HttpClientDownloader
    {
        private HttpResponseMessage? HttpResponseMessage;

        public string? Get(Page page)
        {
            page.HttpStatusCode = null;
            page.InitializeResponseBody();

            var task = Task.Run(async () => await GetAsync(page));
            return task.Result;
        }

        private async Task<string?> GetAsync(Page page)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false,
                // trust certificate allways
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            SetAccepLanguage(httpClient, page);
            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, page.Uri);
            HttpResponseMessage = null;            

            try
            {
                DateTime dateStart = DateTime.Now;
                HttpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                Timers.Timer.SaveTimerDownloadPage(page.Uri.ToString(), dateStart);
                page.HttpStatusCode = (short)HttpResponseMessage.StatusCode;
                return await HttpResponseMessage.Content.ReadAsStringAsync();                
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(page.Uri, exception);
            }
            return null;
        }

        private static void SetAccepLanguage(HttpClient httpClient, Page page)
        {
            switch (page.Website.LanguageCode)
            {
                case LanguageCode.es:
                    {
                        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es-ES"));
                        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 0.9));
                    }
                    break;
            }
        }

        public string? GetRedirectUrl()
        {
            if (HttpResponseMessage != null &&
                HttpResponseMessage.Headers.TryGetValues("Location", out var locations))
            {
                return locations.FirstOrDefault();
            }
            return null;
        }
    }
}
