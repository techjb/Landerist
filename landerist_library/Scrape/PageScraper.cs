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

            var (pageType, listing) = PageTypeParser.GetPageType(Page);
            Page.SetPageType(pageType);
            if (listing != null)
            {
                ES_Listings.InsertUpdate(Page.Website, listing);
            }
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
