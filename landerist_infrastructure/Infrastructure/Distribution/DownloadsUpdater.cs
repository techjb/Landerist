using landerist_library.Application.Listings;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Sql;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using landerist_library.Infrastructure.Runtime;

namespace landerist_library.Infrastructure.Distribution;

public class DownloadsUpdater : DistributionArtifacts
{
    public const string METADATA_KEY_DATEFROM = "dateFrom";
    public const string METADATA_KEY_DATETO = "dateTo";
    public const string METADATA_KEY_COUNTER = "counter";

    private readonly IListingAdministrationService _listings;
    private readonly DownloadsWorkspace _workspace;
    private readonly DownloadsArtifactPublisher _publisher;
    private readonly SegmentedListingsUpdater _segmentedListings;

    public DownloadsUpdater(
        IListingAdministrationService listings,
        DistributionOptions options)
    {
        ArgumentNullException.ThrowIfNull(listings);
        _listings = listings;
        _workspace = new DownloadsWorkspace(options);
        _publisher = new DownloadsArtifactPublisher(Yesterday, options);
        _segmentedListings = new SegmentedListingsUpdater(
            listings,
            _workspace,
            _publisher);
    }

    public void Update(
        IWebsiteCatalog websites,
        WebsiteQueryRepository websiteQueries)
    {
        try
        {
            UpdateWebsites(websiteQueries);
            UpdateFullDataSet(ListingStatus.published);
            UpdateFullDataSet(ListingStatus.unpublished);
            UpdateListingsUpdates();
            UpdateListingsByOperationPropertyType();
            UpdateListingsByWebsite(websites);
        }
        catch (Exception exception)
        {
            Log.WriteError("Update", exception);
        }
    }

    public void UpdateListings()
    {
        Console.WriteLine("Reading all listings ..");
        SortedSet<Listing> listings = _listings.GetAll(true, true);
        if (!WriteAndPublishListings(
            listings,
            CountryCode.ES,
            ExportType.Listings,
            null,
            null))
        {
            Log.WriteError("filesupdater", "Error updating all listings");
        }
    }

    public void UpdateListingsUpdates()
    {
        Console.WriteLine("Reading updates ..");
        DateOnly dateFrom = _publisher.GetDateFrom(
            ExportType.PublishedUpdates,
            ExportType.UnpublishedUpdates);
        DateOnly dateTo = Yesterday();

        UpdateListingsByDateRange(
            ListingStatus.published,
            ExportType.PublishedUpdates,
            dateFrom,
            dateTo);
        UpdateListingsByDateRange(
            ListingStatus.unpublished,
            ExportType.UnpublishedUpdates,
            dateFrom,
            dateTo);
    }

    public void UpdateFullDataSet(ListingStatus listingStatus)
    {
        Console.WriteLine("Reading " + listingStatus + " ..");
        ExportType exportType = listingStatus == ListingStatus.published
            ? ExportType.Published
            : ExportType.Unpublished;
        SortedSet<Listing> listings = _listings.GetByStatus(listingStatus);
        if (!WriteAndPublishListings(
            listings,
            CountryCode.ES,
            exportType,
            null,
            null))
        {
            Log.WriteError("filesupdater", "Error updating " + exportType);
        }
    }

    public bool UpdateWebsites(WebsiteQueryRepository websiteQueries)
    {
        Console.WriteLine("Reading Websites ..");
        var websites = websiteQueries.GetAll();
        if (websites.Rows.Count == 0)
        {
            return true;
        }

        const CountryCode countryCode = CountryCode.ES;
        const ExportType exportType = ExportType.Websites;
        string subdirectory = GetLocalSubdirectory(countryCode, exportType);
        string fileName = GetFileName(countryCode, exportType, "csv");
        if (!_workspace.WriteWebsites(websites, subdirectory, fileName))
        {
            return false;
        }

        string filePath = _workspace.GetArtifactPath(subdirectory, fileName);
        return _publisher.UploadWebsites(filePath, countryCode, websites.Rows.Count);
    }

    public bool UpdateListingsByWebsite(IWebsiteCatalog websites)
        => _segmentedListings.UpdateByWebsite(websites);

    public bool UpdateListingsByOperationPropertyType()
        => _segmentedListings.UpdateByOperationPropertyType();

    private void UpdateListingsByDateRange(
        ListingStatus status,
        ExportType exportType,
        DateOnly dateFrom,
        DateOnly dateTo)
    {
        SortedSet<Listing> listings = _listings.GetByDateRange(
            status,
            true,
            true,
            dateFrom,
            dateTo);
        if (!WriteAndPublishListings(
            listings,
            CountryCode.ES,
            exportType,
            dateFrom,
            dateTo))
        {
            Log.WriteError("filesupdater", "Error updating " + exportType);
        }
    }

    private bool WriteAndPublishListings(
        SortedSet<Listing> listings,
        CountryCode countryCode,
        ExportType exportType,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        Console.WriteLine("Updating " + exportType + "..");
        if (listings.Count == 0)
        {
            return true;
        }

        string subdirectory = GetLocalSubdirectory(countryCode, exportType);
        string fileName = GetFileName(countryCode, exportType, "json");
        if (!_workspace.WriteListings(listings, subdirectory, fileName))
        {
            return false;
        }

        string filePath = _workspace.GetArtifactPath(subdirectory, fileName);
        return _publisher.UploadListings(
            filePath,
            countryCode,
            exportType,
            listings.Count,
            dateFrom,
            dateTo);
    }

    private static DateOnly Yesterday() =>
        DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
}
