using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Download;
using landerist_library.Index;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageScraper(Page page)
    {
        private readonly Page Page = page;

        private readonly HttpClientDownloader HttpClientDownloader = new();

        public bool Scrape()
        {
            if (HttpClientDownloader.Get(Page))
            {
                DownloadSucess();
            }
            else
            {
                DownloadError();
            }
            return Page.Update();
        }

        private void DownloadSucess()
        {
            SetPageType();
            IndexPages();
        }

        private void SetPageType()
        {
            Page.LoadHtmlDocument();
            var (pageType, listing) = PageTypeParser.GetPageType(Page);
            Page.SetPageType(pageType);
            if (listing != null)
            {
                ES_Listings.InsertUpdate(Page.Website, listing);
            }
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }

            if (Page.HtmlDocument == null || Page.Website == null)
            {
                return;
            }
            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).InsertLinksAlternate();
                return;
            }
            if (Page.CanFollowLinks())
            {
                new HyperlinksIndexer(Page).Insert();
            }
        }

        private void DownloadError()
        {
            Page.SetPageType(PageType.DownloadError);

            if (Page.HttpStatusCode == null)
            {
                return;
            }

            int code = (int)Page.HttpStatusCode;
            if (code >= 300 && code < 400 && Config.INDEXER_ENABLED)
            {
                var redirectUrl = HttpClientDownloader.GetRedirectUrl();
                if (redirectUrl != null)
                {
                    new Indexer(Page).InsertUrl(redirectUrl);
                }
            };
        }
    }
}
