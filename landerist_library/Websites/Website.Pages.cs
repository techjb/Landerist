using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public bool Delete()
        {
            DeleteListings();
            Pages.Pages.Delete(this);
            return DeleteWebsite();
        }

        public void DeleteListings()
        {
            int counter = 0;
            var pages = GetPages();

            foreach (var page in pages)
            {
                if (page.DeleteListing())
                {
                    counter++;
                }
            }
        }

        public bool InsertMainPage()
        {
            var page = new Page(this);
            return page.Insert();
        }

        public List<Page> GetPages()
        {
            return Pages.Pages.GetPages(this);
        }

        public List<Page> GetPagesUnknowPageType()
        {
            return Pages.Pages.GetUnknowPageType(this);
        }

        public List<Page> GetNonScrapedPages()
        {
            return Pages.Pages.GetNonScrapedPages(this);
        }

        public int GetNumPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryInt(query, new Dictionary<string, object?> {
                {"Host", Host }
            });
        }       

        public bool AchievedMaxNumberOfPages()
        {
            return 
                //Config.IsConfigurationProduction() &&
                GetNumPages() >= GetMaxPagesPerWebsite();
        }

        private int GetMaxPagesPerWebsite()
        {
            return ApplySpecialRules
                ? Config.MAX_PAGES_PER_WEBSITE_SPECIAL_RULES
                : Config.MAX_PAGES_PER_WEBSITE;
        }

        public int GetNumListings()
        {
            return ES_Listings.Count(Host);
        }

        public int GetNumPublishedListings()
        {
            return ES_Listings.Count(Host, ListingStatus.published);
        }

        public int GetNumPublishedListingsWithAddress()
        {
            return ES_Listings.CountWithAddress(Host, ListingStatus.published);
        }

        public int GetNumPublishedListingsWithCoordinates()
        {
            return ES_Listings.CountWithCoordinates(Host, ListingStatus.published);
        }

        public int GetNumPublishedListingsWithImages()
        {
            return ES_Listings.CountWithImages(Host, ListingStatus.published);
        }

        public int GetNumUnpublishedListings()
        {
            return ES_Listings.Count(Host, ListingStatus.unpublished);
        }
    }
}
