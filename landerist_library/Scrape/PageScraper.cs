﻿using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Parse.Location;
using landerist_library.Parse.Media;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Websites;
using landerist_orels.ES;
using landerist_library.Downloaders.Multiple;

namespace landerist_library.Scrape
{
    public class PageScraper(Page page)
    {
        private readonly Page Page = page;

        private Listing? NewListing;

        private readonly PageType? OldPageType = page.PageType;

        private readonly SingleDownloader? Downloader;

        public PageScraper(Page page, Scraper scraper) : this(page)
        {
            Downloader = scraper.MultipleDownloader.GetDownloader();
        }
        public bool Scrape()
        {
            if (Downloader == null)
            {
                return false;
            }
            Downloader.Download(Page);
            (var newPageType, var newListing, var waitingAIParsing) = PageTypeParser.GetPageType(Page);
            if(!Config.IsConfigurationProduction())
            {
                Console.WriteLine("PageType: " + newPageType);
            }
            bool sucess =  SetPageType(newPageType, newListing, waitingAIParsing);
            IndexPages();
            return sucess;
        }

        public bool SetPageType(PageType? newPageType, Listing? newListing, bool waitingAIParsing)
        {
            if (waitingAIParsing)
            {
                Page.SetWaitingAIRequest();
                return Page.Update(false);
            }
            return SetPageType(newPageType, newListing);
        }

        public bool SetPageType(PageType? newPageType, Listing? newListing)
        {
            NewListing = newListing;
            Page.SetPageType(newPageType);
            UpdateListing();
            return Page.Update(true);
        }


        private void UpdateListing()
        {
            if (Page.PageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(true);
                SetMedia();
                SetLocation();
                UpdateNewListing();
                return;
            }

            if (!Page.PageType.Equals(PageType.MayBeListing) &&
                OldPageType != null &&
                OldPageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(false);
                if (NewListing != null)
                {
                    NewListing.listingStatus = ListingStatus.unpublished;
                    NewListing.unlistingDate = DateTime.Now;
                    UpdateNewListing();
                }
            }
        }

        private void SetMedia()
        {
            if (NewListing != null)
            {
                MediaParser mediaParser = new(Page);
                mediaParser.AddMedia(NewListing);
            }
        }

        private void SetLocation()
        {
            if (NewListing != null)
            {
                LatLngParser latLngParser = new(Page, NewListing);
                latLngParser.SetLatLng();
            }
        }

        private void UpdateNewListing()
        {
            if (NewListing != null)
            {
                ES_Listings.InsertUpdate(Page.Website, NewListing);
            }
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

            if (Downloader == null)
            {
                return;
            }
            var redirectUrl = Downloader.GetRedirectUrl();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                new Indexer(Page).Insert(redirectUrl);
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
