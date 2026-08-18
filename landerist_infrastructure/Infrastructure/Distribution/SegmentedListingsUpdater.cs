using landerist_library.Application.Listings;
using landerist_library.Application.Websites;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using static landerist_library.Infrastructure.Distribution.DistributionArtifactNaming;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class SegmentedListingsUpdater
{
    private const string HostsSubdirectory = "ES\\Hosts";
    private const string OperationPropertyTypesSubdirectory =
        "ES\\OperationPropertyTypes";

    private readonly IListingAdministrationService _listings;
    private readonly DownloadsWorkspace _workspace;
    private readonly DownloadsArtifactPublisher _publisher;

    public SegmentedListingsUpdater(
        IListingAdministrationService listings,
        DownloadsWorkspace workspace,
        DownloadsArtifactPublisher publisher)
    {
        _listings = listings;
        _workspace = workspace;
        _publisher = publisher;
    }

    public bool UpdateByWebsite(IWebsiteCatalog websites)
    {
        Console.WriteLine("Reading hosts ..");
        var websiteList = websites.GetAll()
            .OrderBy(website => website.Host, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (websiteList.Count == 0)
        {
            return true;
        }

        _workspace.EnsureSubdirectory(HostsSubdirectory);
        foreach (Website website in websiteList)
        {
            if (!UpdateHost(website, ListingStatus.published) ||
                !UpdateHost(website, ListingStatus.unpublished))
            {
                Log.WriteError(
                    "filesupdater",
                    "Error updating host files: " + website.Host);
                return false;
            }
        }

        return true;
    }

    public bool UpdateByOperationPropertyType()
    {
        Console.WriteLine("Reading listings by operation and property type ..");
        _workspace.EnsureSubdirectory(OperationPropertyTypesSubdirectory);

        foreach (Operation operation in Enum.GetValues<Operation>())
        {
            foreach (PropertyType propertyType in Enum.GetValues<PropertyType>())
            {
                if (!UpdateOperationPropertyType(
                        operation,
                        propertyType,
                        ListingStatus.published) ||
                    !UpdateOperationPropertyType(
                        operation,
                        propertyType,
                        ListingStatus.unpublished))
                {
                    Log.WriteError(
                        "filesupdater",
                        $"Error updating operation/property type files: {operation} {propertyType}");
                    return false;
                }
            }
        }

        return true;
    }

    private bool UpdateOperationPropertyType(
        Operation operation,
        PropertyType propertyType,
        ListingStatus listingStatus)
    {
        SortedSet<Listing> listings = _listings.GetByStatus(
            listingStatus,
            operation,
            propertyType,
            true,
            true);
        if (listings.Count == 0)
        {
            return true;
        }

        string fileName = GetListingsByOperationPropertyTypeFileName(
            CountryCode.ES,
            operation,
            propertyType,
            listingStatus,
            "json");
        if (!_workspace.WriteListings(
            listings,
            OperationPropertyTypesSubdirectory,
            fileName))
        {
            return false;
        }

        string filePath = _workspace.GetArtifactPath(
            OperationPropertyTypesSubdirectory,
            fileName);
        return _publisher.UploadOperationPropertyType(
            filePath,
            operation,
            propertyType,
            listingStatus,
            listings.Count,
            OperationPropertyTypesSubdirectory);
    }

    private bool UpdateHost(Website website, ListingStatus listingStatus)
    {
        SortedSet<Listing> listings = _listings.GetByHost(website.Host, listingStatus);
        if (listings.Count == 0)
        {
            return true;
        }

        string fileName = GetHostListingsFileName(
            CountryCode.ES,
            website.Host,
            listingStatus,
            "json");
        if (!_workspace.WriteListings(listings, HostsSubdirectory, fileName))
        {
            return false;
        }

        string filePath = _workspace.GetArtifactPath(HostsSubdirectory, fileName);
        return _publisher.UploadHost(
            filePath,
            website.Host,
            listingStatus,
            listings.Count,
            HostsSubdirectory);
    }
}
