using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Listings
{
    public sealed class SqlListingAdministrationService : IListingAdministrationService
    {
        private readonly IListingRecordRepository Repository;
        private readonly ListingQueryRepository QueryRepository;
        private readonly ListingStatisticsRepository StatisticsRepository;
        private readonly SqlListingMediaStore Media;
        private readonly SqlListingSourceStore Sources;
        private readonly IApplicationLogger Logger;

        public SqlListingAdministrationService(
            IListingRecordRepository repository,
            ListingQueryRepository queryRepository,
            ListingStatisticsRepository statisticsRepository,
            IListingMediaRepository mediaRepository,
            SourceRepository sourceRepository,
            IApplicationLogger logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(queryRepository);
            ArgumentNullException.ThrowIfNull(statisticsRepository);
            ArgumentNullException.ThrowIfNull(mediaRepository);
            ArgumentNullException.ThrowIfNull(sourceRepository);
            ArgumentNullException.ThrowIfNull(logger);
            Repository = repository;
            QueryRepository = queryRepository;
            StatisticsRepository = statisticsRepository;
            Media = new SqlListingMediaStore(mediaRepository);
            Sources = new SqlListingSourceStore(sourceRepository);
            Logger = logger;
        }

        public void Upsert(Website website, Listing newListing, ListingUnpublishDecision? unpublishDecision = null)
        {
            Listing? oldListing = Get(newListing.guid, true, true);
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

        private bool Insert(Website website, Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            if (Insert(listing, website.Host, unpublishDecision))
            {
                Media.Insert(listing);
                Sources.Insert(listing);
                return true;
            }

            Logger.WriteError("SqlListingAdministrationService", "Insert error");
            return false;
        }

        private bool Insert(Listing listing, string host, ListingUnpublishDecision? unpublishDecision)
        {
            bool inserted = Repository.Insert(listing, host, unpublishDecision, out Exception? exception);
            if (!inserted && exception != null)
            {
                Logger.WriteError(
                    "SqlListingAdministrationService Insert",
                    $"Guid: {listing.guid} Host: {host}. {exception}");
            }
            return inserted;
        }

        public int Count(string host)
        {
            return StatisticsRepository.Count(host);
        }

        public int Count(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.Count(host, listingStatus);
        }

        public int CountSinceListingDate(string host, DateTime listingDateFrom)
        {
            return StatisticsRepository.CountSinceListingDate(host, listingDateFrom);
        }

        public int CountWithAddress(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithAddress(host, listingStatus);
        }

        public int CountWithCoordinates(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithCoordinates(host, listingStatus);
        }

        public int CountWithImages(string host, ListingStatus listingStatus)
        {
            return StatisticsRepository.CountWithImages(host, listingStatus);
        }

        public int Count(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
        {
            return StatisticsRepository.Count(listingStatus, operation, propertyType);
        }

        private bool Update(Listing oldListing, Listing newListing, ListingUnpublishDecision? unpublishDecision)
        {
            if (!Update(newListing, unpublishDecision))
            {
                Logger.WriteError("SqlListingAdministrationService", "Update error");
                return false;
            }
            if (!ListingMediaAreEquals(oldListing, newListing))
            {
                Media.Update(newListing);
            }
            if (!ListingSourcesAreEquals(oldListing, newListing))
            {
                Sources.Update(newListing);
            }
            return true;
        }

        private bool ListingMediaAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.media == newListing.media ||
                (oldListing.media != null && newListing.media != null && oldListing.media.SetEquals(newListing.media));
        }

        private bool ListingSourcesAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.sources == newListing.sources ||
                (oldListing.sources != null && newListing.sources != null && oldListing.sources.SetEquals(newListing.sources));
        }

        public bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null)
        {
            return Repository.Update(listing, unpublishDecision);
        }

        public Task<bool> UpdateAsync(Listing listing, ListingUnpublishDecision? unpublishDecision = null, CancellationToken cancellationToken = default) =>
            Repository.UpdateAsync(listing, unpublishDecision, cancellationToken);

        public SortedSet<Listing> GetAll(bool loadMedia, bool loadSources)
        {
            return GetAll(QueryRepository.GetAll(), loadMedia, loadSources);
        }

        public SortedSet<Listing> GetPublished()
        {
            return GetByStatus(ListingStatus.published);
        }

        public SortedSet<Listing> GetUnPublished()
        {
            return GetByStatus(ListingStatus.unpublished);
        }

        public SortedSet<Listing> GetByStatus(ListingStatus listingStatus, bool loadMedia = true, bool loadSources = true)
        {
            return GetAll(QueryRepository.GetListings(listingStatus), loadMedia, loadSources);
        }

        public SortedSet<Listing> GetByStatus(
            ListingStatus listingStatus,
            Operation operation,
            PropertyType propertyType,
            bool loadMedia,
            bool loadSources)
        {
            return GetAll(QueryRepository.GetListings(listingStatus, operation, propertyType), loadMedia, loadSources);
        }

        public SortedSet<Listing> GetByHost(string host, ListingStatus? listingStatus = null)
        {
            return GetAll(QueryRepository.GetListings(host, listingStatus), true, true);
        }

        public SortedSet<Listing> GetWithCadastralReference()
        {
            return GetAll(QueryRepository.GetListingWithCatastralReference(), false, false);
        }

        public SortedSet<Listing> GetWithoutLauName()
        {
            return GetAll(QueryRepository.GetListingsWithoutLauName(), false, false);
        }

        public SortedSet<Listing> GetWithCadastralReferenceAndNoAddress()
        {
            return GetAll(QueryRepository.GetListingWithCatastralReferenceAndNoAddress(), false, false);
        }

        public SortedSet<Listing> GetWithoutCadastralReferenceAndAccurateLocation()
        {
            return GetAll(QueryRepository.GetListingsWithoutCatastralReferenceAndLocationIsAccurate(), false, true);
        }

        public SortedSet<Listing> GetAccurateLocationWithoutCadastralReference()
        {
            return GetAll(QueryRepository.GetListingsLocationIsAccurateNoCadastralReference(), false, false);
        }

        public SortedSet<Listing> GetUnpublishedListings(DateTime unlistingDate)
        {
            return ParseListings(QueryRepository.GetUnpublishedListings(unlistingDate), false, true);
        }

        public SortedSet<Listing> GetByDateRange(bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo)
        {
            return GetAll(QueryRepository.GetListings(dateFrom, dateTo), loadMedia, loadSources);
        }

        public SortedSet<Listing> GetByDateRange(ListingStatus listingStatus, bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo)
        {
            return GetAll(QueryRepository.GetListings(listingStatus, dateFrom, dateTo), loadMedia, loadSources);
        }

        private SortedSet<Listing> ParseListings(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = Get(dataRow, loadMedia, loadSources);
                listings.Add(listing);
            }
            return listings;
        }

        private SortedSet<Listing> GetAll(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            Parallel.ForEach(dataTable.AsEnumerable(), new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, dataRow =>
            {
                var listing = Get(dataRow, loadMedia, loadSources);
                lock (listings)
                {
                    listings.Add(listing);
                }
            });
            return listings;
        }

        public Listing? Get(Page page, bool loadMedia, bool loadSources)
        {
            return Get(page.UriHash, loadMedia, loadSources);
        }

        public Listing? Get(string guid)
        {
            return Get(guid, false, false);
        }

        public Listing? Get(string guid, bool loadMedia, bool loadSources)
        {
            DataTable dataTable = QueryRepository.GetListing(guid);

            if (dataTable.Rows.Count.Equals(1))
            {
                return Get(dataTable.Rows[0], loadMedia, loadSources);
            }
            return null;
        }

        private Listing Get(DataRow dataRow, bool loadMedia, bool loadSources)
        {
            var listing = GetListingData(dataRow);
            if (loadMedia)
            {
                var media = Media.GetMedia(listing);
                listing.SetMedia(media);
            }
            if (loadSources)
            {
                var sources = Sources.GetSources(listing);
                listing.SetSources(sources);
            }
            return listing;
        }

        internal Listing GetListingData(DataRow dataRow) =>
            ListingDataMapper.Map(dataRow);

        public bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public bool Delete(string guid)
        {
            return Repository.Delete(guid);
        }

        public Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default) =>
            Repository.DeleteAsync(guid, cancellationToken);

        public bool DeleteAll()
        {
            return Repository.DeleteAll();
        }

        public Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default) =>
            Repository.DeleteAllAsync(cancellationToken);

        public void RepairListingsWithoutSources()
        {
            DataTable rows = Sources.GetListingsWithoutSourcePages();
            int total = rows.Rows.Count;
            int processed = 0;
            int errors = 0;

            Parallel.ForEach(rows.AsEnumerable(), row =>
            {
                string uri = row["Uri"].ToString() ?? string.Empty;
                using var page = new Page(uri);
                Listing? listing = Get(page, loadMedia: true, loadSources: true);
                if (listing is null)
                {
                    Interlocked.Increment(ref errors);
                    return;
                }

                listing.sources.Add(new Source
                {
                    sourceGuid = string.Empty,
                    sourceUrl = page.Uri,
                    sourceName = page.Website.Host,
                });
                Upsert(page.Website, listing);
                int current = Interlocked.Increment(ref processed);
                Console.WriteLine($"{current}/{total} Errors: {errors}");
            });
        }

        public bool UpdateAddress(string guid, double? latitude, double? longitude, bool? locationIsAccurate, string? locationResolver = null)
        {
            return Repository.UpdateAddress(guid, latitude, longitude, locationIsAccurate, locationResolver);
        }

        public Task<bool> UpdateAddressAsync(string guid, double? latitude, double? longitude, bool? locationIsAccurate, string? locationResolver = null, CancellationToken cancellationToken = default) =>
            Repository.UpdateAddressAsync(guid, latitude, longitude, locationIsAccurate, locationResolver, cancellationToken);
    }
}
