using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Parse;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageScraper
    {
        private readonly Page Page;

        private HttpResponseMessage? HttpResponseMessage;
        public PageScraper(Page page)
        {
            Page = page;
        }

        public bool Scrape()
        {
            var task = Task.Run(async () => await Download());
            if (task.Result)
            {
                DownloadSucess();
            }
            else
            {
                DownloadError();
            }
            return Page.Update();
        }

        private async Task<bool> Download()
        {
            Page.HttpStatusCode = null;
            Page.ResponseBody = null;
            Page.ResponseBodyText = null;

            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);

            HttpRequestMessage request = new(HttpMethod.Get, Page.Uri);
            bool sucess = false;
            try
            {
                HttpResponseMessage = await client.SendAsync(request);
                sucess = HttpResponseMessage.IsSuccessStatusCode;
                Page.HttpStatusCode = (short)HttpResponseMessage.StatusCode;
                Page.ResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(Page.Uri, exception);
            }
            return sucess;
        }

        private void DownloadSucess()
        {
            InsertPages();
            GetListing();
        }

        private void InsertPages()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument == null || Page.Website == null)
            {
                return;
            }
            new Indexer(Page).InsertPageUrls();
        }

        private void GetListing()
        {
            if (!Page.CanRequestListing())
            {
                return;
            }
            var listingParser = new ListingParser(Page).GetListing();
            Page.IsListing = listingParser.Item1;
            var listing = listingParser.Item2;
            if (listing != null)
            {
                new LocationParser(Page, listing).SetLocation();
                new MediaParser(Page).AddMedia(listing);                
                ES_Listings.InsertUpdate(listing);
            }
        }

        private void DownloadError()
        {
            if (Page.HttpStatusCode == null)
            {
                return;
            }
            int code = (int)Page.HttpStatusCode;
            if(code >= 300 && code < 400)
            {
                DownloadErrorRedirect();
            };
        }

        private void DownloadErrorRedirect()
        {
            if (HttpResponseMessage == null)
            {
                return;
            }
            if (HttpResponseMessage.Headers.TryGetValues("Location", out var locations))
            {
                var redirectUrl = locations.FirstOrDefault();
                if (redirectUrl != null)
                {
                    new Indexer(Page).InsertUrl(redirectUrl);
                }                
            }            
        }
    }
}
