using landerist_library.Configuration;
using landerist_library.Websites;
using System.Net.Http.Headers;

namespace landerist_library.Download
{
    public class HttpClientDownloader
    {
        private HttpResponseMessage? HttpResponseMessage;


        public bool Get(Page page)
        {
            page.HttpStatusCode = null;
            page.ResponseBody = null;
            page.ResponseBodyText = null;

            var task = Task.Run(async () => await GetAsync(page));
            return task.Result;
        }

        private async Task<bool> GetAsync(Page page)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false,
                // trust certificate allways
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es-ES"));
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 0.9));

            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            HttpRequestMessage request = new(HttpMethod.Get, page.Uri);
            
            bool sucess = false;
            HttpResponseMessage = null;

            try
            {
                DateTime dateStart = DateTime.Now;
                HttpResponseMessage = await httpClient.SendAsync(request);
                Timers.Timer.SaveTimerDownloadPage(page.Uri.ToString(), dateStart);
                sucess = HttpResponseMessage.IsSuccessStatusCode;
                page.HttpStatusCode = (short)HttpResponseMessage.StatusCode;
                page.ResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync();                

            }          
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(page.Uri, exception);
            }
            return sucess;
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
