using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Index;
using landerist_library.Parse.Location;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class PageScraper(Page page)
    {
        private readonly Page Page = page;

        private Listing? NewListing;

        private SingleDownloader? SingleDownloader;

        private readonly Scraper? Scraper;

        private readonly bool UseProxy = false;


        public PageScraper(Page page, Scraper scraper, bool useProxy) : this(page)
        {
            Scraper = scraper;
            UseProxy = useProxy;
        }

        public bool Scrape()
        {
            if (!Download())
            {
                //Console.WriteLine("PageScraper Download failed for: " + Page.Uri);
                return false;
            }
            (var newPageType, var newListing, var waitingAIRequest) = new PageTypeParser(Page).GetPageType();
            bool sucess = SetPageType(newPageType, newListing, waitingAIRequest);
            IndexPages();
            return sucess;
        }


        private bool Download()
        {
            if (Config.MULTIPLE_DOWNLOADERS_ENABLED)
            {
                return DownloadMultipleDownloaders();
            }
            return DownloadSingleDownloader();
        }

        private bool DownloadMultipleDownloaders()
        {
            if (Scraper is null)
            {
                return false;
            }
            SingleDownloader = MultipleDownloader.GetDownloader(UseProxy);
            if (SingleDownloader is null)
            {
                return false;
            }
            return SingleDownloader.Download(Page);
        }

        private bool DownloadSingleDownloader()
        {
            SingleDownloader = new(UseProxy);
            if (!SingleDownloader.IsAvailable(UseProxy))
            {
                Logs.Log.WriteInfo("PageScraper DownloadPageSingleDownloader", "Downloader not available");
                return false;
            }
            SingleDownloader.Download(Page);
            SingleDownloader.CloseBrowser();
            return true;
        }

        public bool SetPageType(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            if (waitingAIRequest)
            {
                if (!Page.SetResponseBodyZipped())
                {
                    Logs.Log.WriteError("PageScraper SetPageType", "Failed to set response body zipped");
                    return false;
                }
                Page.SetWaitingStatusAIRequest();
            }

            SetPageType(newPageType, newListing);
            var setNextUpdate = !waitingAIRequest;
            return Page.Update(setNextUpdate);
        }

        public bool SetPageTypeAfterParsing(PageType newPageType, Listing? listing)
        {
            if (newPageType.Equals(PageType.MayBeListing))
            {
                return false;
            }
            Page.RemoveWaitingStatus();
            Page.SetResponseBodyFromZipped();
            SetPageType(newPageType, listing);

            new TrainingData().Insert(Page); // todo: remove when finshed testing

            Page.RemoveResponseBodyZipped();
            return Page.Update(true);
        }


        public void SetPageType(PageType? newPageType, Listing? newListing)
        {
            NewListing = newListing;
            Page.SetPageType(newPageType);

            if (Page.IsListing())
            {
                HandleLPublishedListing();
                return;
            }
            if (Page.IsNotListingByParser())
            {
                Page.InsertNotListingResponseBodyText();
            }
            if (Page.HaveToUnpublishListing())
            {
                HandleUnpublishedListing();
            }
            if (Page.IsNotCanonicalListing() || Page.IsRedirectToAnotherUrlListing())
            {
                // todo: handle not canonical listing
            }
        }

        
        private void HandleLPublishedListing()
        {
            NewListing ??= Page.GetListing(true, true);
            if (NewListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleLPublishedListing", "NewListing is null");
                return;
            }

            NewListing.SetPublished();
            new LocationParser(Page, NewListing).SetLatLng();
            new LauIdParser(Page.Website.CountryCode, NewListing).SetLauIdAndLauName();
            ES_Listings.InsertUpdate(Page.Website, NewListing);
            Page.SetListingStatusPublished();
        }

        private void HandleUnpublishedListing()
        {
            NewListing ??= Page.GetListing(true, true);
            if (NewListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
                return;
            }

            NewListing.SetUnpublished();
            ES_Listings.InsertUpdate(Page.Website, NewListing);
            Page.SetListingStatusUnpublished();
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            if (Page.Website.AchievedMaxNumberOfPages())
            {
                return;
            }
            if (!string.IsNullOrEmpty(Page.RedirectUrl))
            {
                new Indexer(Page).Insert(Page.RedirectUrl);
                return;
            }
            if (Page.ContainsMetaRobotsNoFollow())
            {
                return;
            }
            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).Insert();
                return;
            }
            if (Page.PageType.Equals(PageType.NotCanonical))
            {
                new CanonicalIndexer(Page).Insert();
                return;
            }
            new HyperlinksIndexer(Page).Insert();
        }

        public Listing? GetListing()
        {
            return NewListing;
        }
    }
}
