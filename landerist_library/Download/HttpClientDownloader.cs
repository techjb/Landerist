using landerist_library.Configuration;
using landerist_library.Websites;
using System.Net.Http.Headers;
using System.Text;

namespace landerist_library.Download
{
    public class HttpClientDownloader
    {
        private HttpResponseMessage? HttpResponseMessage;

        public string? Get(Page page)
        {
            page.HttpStatusCode = null;
            page.InitializeResponseBody();
            
            var result = GetAsync(page.Website, page.Uri);
            if(HttpResponseMessage != null)
            {
                page.HttpStatusCode = (short)HttpResponseMessage.StatusCode;
            }            
            return result.Result;
        }
        
        public async Task<string?> GetAsync(Website website, Uri uri)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false,
                // trust certificate allways
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);            

            SetAccepLanguage(httpClient, website.LanguageCode);
            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
            HttpResponseMessage = null;

            try
            {
                DateTime dateStart = DateTime.Now;                
                HttpResponseMessage = await httpClient.SendAsync(httpRequestMessage);                
                Timers.Timer.SaveTimerDownloadPage(uri.ToString(), dateStart);
                return await HttpResponseMessage.Content.ReadAsStringAsync();
            }
            catch// (Exception exception)
            {
                //Logs.Log.WriteLogErrors("HttpClientDownloader GetAsync", page.Uri, exception);
            }
            return null;
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
