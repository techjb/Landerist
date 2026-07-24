using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Listings
    {
        public const string TABLE_ES_LISTINGS = "[ES_LISTINGS]";
        private static readonly ListingRepository Repository = new(global::landerist_library.Database.LegacyDatabase.Create());
        private static readonly ListingQueryRepository QueryRepository = new(global::landerist_library.Database.LegacyDatabase.Create());
        private static readonly ListingStatisticsRepository StatisticsRepository = new(global::landerist_library.Database.LegacyDatabase.Create());

        public static void InsertUpdate(Website website, Listing newListing, ListingUnpublishDecision? unpublishDecision = null)
        {
            Listing? oldListing = GetListing(newListing.guid, true, true);
            if (oldListing != null)
            {
                if (!oldListing.Equals(newListing))
                {
                    Update(oldListing, newListing, unpublishDecision);
                }
            }
            else
            {
                Insert(website, newListing, unpublishDecision);
            }
        }

        private static bool Insert(Website website, Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            if (Insert(listing, website.Host, unpublishDecision))
            {
                ES_Media.Insert(listing);
                ES_Sources.Insert(listing);
                return true;
            }

            Logs.Log.WriteError("ES_Listings", "Insert error");
            return false;
        }

        private static bool Insert(Listing listing, string host, ListingUnpublishDecision? unpublishDecision)
        {
            bool inserted = Repository.Insert(listing, host, unpublishDecision, out Exception? exception);
            if (!inserted && exception != null)
            {
                Logs.Log.WriteError("ES_Listings Insert", "Guid: " + listing.guid + " Host: " + host, exception);
            }
            return inserted;
        }

        public static int Count(string host)
        {
            return StatisticsRepository.Count(host);
        }

        public static int Count(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.Count(host, listingStatus);
        }

        public static int CountSinceListingDate(string host, DateTime listingDateFrom)
        {
            return StatisticsRepository.CountSinceListingDate(host, listingDateFrom);
        }

        public static int CountWithAddress(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithAddress(host, listingStatus);
        }

        public static int CountWithCoordinates(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithCoordinates(host, listingStatus);
        }

        public static int CountWithImages(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithImages(host, listingStatus);
        }

        public static int Count(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
        {
            return StatisticsRepository.Count(listingStatus, operation, propertyType);
        }

        private static bool Update(Listing oldListing, Listing newListing, ListingUnpublishDecision? unpublishDecision)
        {
            if (!Update(newListing, unpublishDecision))
            {
                Logs.Log.WriteError("ES_Listings", "Update error");
                return false;
            }
            if (!ListingMediaAreEquals(oldListing, newListing))
            {
                ES_Media.Update(newListing);
            }
            if (!ListingSourcesAreEquals(oldListing, newListing))
            {
                ES_Sources.Update(newListing);
            }
            return true;
        }

        private static bool ListingMediaAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.media == newListing.media ||
                (oldListing.media != null && newListing.media != null && oldListing.media.SetEquals(newListing.media));
        }

        private static bool ListingSourcesAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.sources == newListing.sources ||
                (oldListing.sources != null && newListing.sources != null && oldListing.sources.SetEquals(newListing.sources));
        }

        public static bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null)
        {
            return Repository.Update(listing, unpublishDecision);
        }

        public static SortedSet<Listing> GetAll(bool loadMedia, bool loadSources)
        {
            return GetAll(QueryRepository.GetAll(), loadMedia, loadSources);
        }

        public static SortedSet<Listing> GetPublished()
        {
            return GetListings(ListingStatus.published);
        }

        public static SortedSet<Listing> GetUnPublished()
        {
            return GetListings(ListingStatus.unpublished);
        }

        public static SortedSet<Listing> GetListings(ListingStatus listingStatus)
        {
            return GetAll(QueryRepository.GetListings(listingStatus), true, true);
        }

        public static SortedSet<Listing> GetListings(
            ListingStatus listingStatus,
            Operation operation,
            PropertyType propertyType,
            bool loadMedia,
            bool loadSources)
        {
            return GetAll(QueryRepository.GetListings(listingStatus, operation, propertyType), loadMedia, loadSources);
        }

        public static SortedSet<Listing> GetListings(string host, ListingStatus? listingStatus = null)
        {
            return GetAll(QueryRepository.GetListings(host, listingStatus), true, true);
        }

        public static SortedSet<Listing> GetListingWithCatastralReference()
        {
            return GetAll(QueryRepository.GetListingWithCatastralReference(), false, false);
        }

        public static SortedSet<Listing> GetListingsWithoutLauName()
        {
            return GetAll(QueryRepository.GetListingsWithoutLauName(), false, false);
        }

        public static SortedSet<Listing> GetListingWithCatastralReferenceAndNoAddress()
        {
            return GetAll(QueryRepository.GetListingWithCatastralReferenceAndNoAddress(), false, false);
        }

        public static SortedSet<Listing> GetListingsWithoutCatastralReferenceAndLocationIsAccurate()
        {
            return GetAll(QueryRepository.GetListingsWithoutCatastralReferenceAndLocationIsAccurate(), false, true);
        }

        public static SortedSet<Listing> GetListingsLocationIsAccurateNoCadastralReference()
        {
            return GetAll(QueryRepository.GetListingsLocationIsAccurateNoCadastralReference(), false, false);
        }

        public static SortedSet<Listing> GetUnpublishedListings(DateTime unlistingDate)
        {
            return ParseListings(QueryRepository.GetUnpublishedListings(unlistingDate), false, true);
        }

        public static SortedSet<Listing> GetListings(bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo)
        {
            return GetAll(QueryRepository.GetListings(dateFrom, dateTo), loadMedia, loadSources);
        }

        public static SortedSet<Listing> GetListings(ListingStatus listingStatus, bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo)
        {
            return GetAll(QueryRepository.GetListings(listingStatus, dateFrom, dateTo), loadMedia, loadSources);
        }

        private static SortedSet<Listing> ParseListings(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = GetListing(dataRow, loadMedia, loadSources);
                listings.Add(listing);
            }
            return listings;
        }

        private static SortedSet<Listing> GetAll(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            Parallel.ForEach(dataTable.AsEnumerable(), new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, dataRow =>
            {
                var listing = GetListing(dataRow, loadMedia, loadSources);
                lock (listings)
                {
                    listings.Add(listing);
                }
            });
            return listings;
        }

        public static Listing? GetListing(Page page, bool loadMedia, bool loadSources)
        {
            return GetListing(page.UriHash, loadMedia, loadSources);
        }

        public static Listing? GetListing(string guid)
        {
            return GetListing(guid, false, false);
        }

        public static Listing? GetListing(string guid, bool loadMedia, bool loadSources)
        {
            DataTable dataTable = QueryRepository.GetListing(guid);

            if (dataTable.Rows.Count.Equals(1))
            {
                return GetListing(dataTable.Rows[0], loadMedia, loadSources);
            }
            return null;
        }

        private static Listing GetListing(DataRow dataRow, bool loadMedia, bool loadSources)
        {
            var listing = GetListingData(dataRow);
            if (loadMedia)
            {
                var media = ES_Media.GetMedia(listing);
                listing.SetMedia(media);
            }
            if (loadSources)
            {
                var sources = ES_Sources.GetSources(listing);
                listing.SetSources(sources);
            }
            return listing;
        }

        internal static Listing GetListingData(DataRow dataRow) =>
            ListingDataMapper.Map(dataRow);

        public static bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public static bool Delete(string guid)
        {
            return Repository.Delete(guid);
        }

        public static bool Delete()
        {
            return Repository.Delete();
        }

        public bool UpdateAddress(string guid, double? latitude, double? longitude, bool? locationIsAccurate, string? locationResolver = null)
        {
            return Repository.UpdateAddress(guid, latitude, longitude, locationIsAccurate, locationResolver);
        }
    }
}
