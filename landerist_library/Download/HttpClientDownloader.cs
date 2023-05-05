using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Download
{
    public class HttpClientDownloader
    {
        private short? HttpStatusCode;

        private string? ResponseBody;

        private HttpResponseMessage? HttpResponseMessage;

        public bool Get(Page page)
        {
            page.HttpStatusCode = null;
            page.ResponseBody = null;
            page.ResponseBodyText = null;

            if (Get(page.Uri))
            {
                page.HttpStatusCode = HttpStatusCode;
                page.ResponseBody = ResponseBody;
                return true;
            }
            return false;
        }

        public bool Get(Uri uri)
        {
            HttpStatusCode = null;
            ResponseBody = null;

            var task = Task.Run(async () => await GetAsync(uri));
            return task.Result;
        }

        private async Task<bool> GetAsync(Uri uri)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);

            HttpRequestMessage request = new(HttpMethod.Get, uri);
            bool sucess = false;
            HttpResponseMessage = null;
            try
            {
                HttpResponseMessage = await client.SendAsync(request);
                sucess = HttpResponseMessage.IsSuccessStatusCode;
                HttpStatusCode = (short)HttpResponseMessage.StatusCode;
                ResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(uri, exception);
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
