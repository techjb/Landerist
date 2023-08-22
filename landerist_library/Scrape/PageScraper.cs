using landerist_library.Configuration;
using landerist_library.Download;
using landerist_library.Index;
using landerist_library.Parse.PageType;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageScraper
    {
        private readonly Page Page;

        private readonly HttpClientDownloader HttpClientDownloader;

        public PageScraper(Page page)
        {
            Page = page;
            HttpClientDownloader = new HttpClientDownloader();
        }

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
            if (!IsCorrectLanguage())
            {
                Page.SetPageType(PageType.IncorrectLanguage);
                if (Config.INDEXER_ENABLED)
                {
                    new LinkAlternateIndexer(Page).InsertLinksAlternate();
                }
                return;
            }
            if (Page.CanFollowLinks())
            {
                IndexPages();
            }
            SetPageType();
        }

        private bool IsCorrectLanguage()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                var htmlNode = Page.HtmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return LanguageValidator.IsValidLanguageAndCountry(Page.Website, lang.Value);
                    }
                }
            }
            return true;
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument == null || Page.Website == null)
            {
                return;
            }
            new HyperlinksIndexer(Page).Insert();
        }

        private void SetPageType()
        {
            var pageType = PageTypeParser.GetPageType(Page);
            Page.SetPageType(pageType);
        }

        private void DownloadError()
        {
            Page.SetPageType(PageType.DownloadError);

            if (Page.HttpStatusCode == null)
            {
                return;
            }

            int code = (int)Page.HttpStatusCode;
            if (code >= 300 && code < 400)
            {
                DownloadErrorRedirect();
            };
        }

        private void DownloadErrorRedirect()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            var redirectUrl = HttpClientDownloader.GetRedirectUrl();
            if (redirectUrl != null)
            {
                new Indexer(Page).InsertUrl(redirectUrl);
            }
        }
    }
}
