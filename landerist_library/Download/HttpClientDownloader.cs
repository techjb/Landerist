using landerist_library.Configuration;
using landerist_library.Websites;

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

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            client.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            HttpRequestMessage request = new(HttpMethod.Get, page.Uri);
            bool sucess = false;
            HttpResponseMessage = null;

            try
            {
                DateTime dateStart = DateTime.Now;
                HttpResponseMessage = await client.SendAsync(request);
                Timers.Timer.DownloadPage(page.Uri.ToString(), dateStart);
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
