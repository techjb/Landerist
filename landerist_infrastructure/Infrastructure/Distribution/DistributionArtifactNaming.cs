using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Infrastructure.Distribution;

internal static class DistributionArtifactNaming
{
    public static string GetLocalSubdirectory(CountryCode countryCode, ExportType exportType) =>
        countryCode + "\\" + exportType;

    public static string GetFileName(
        CountryCode countryCode,
        ExportType exportType,
        string fileExtension) =>
        GetFileName(countryCode, exportType) + "." + fileExtension;

    public static string GetFileName(CountryCode countryCode, ExportType exportType)
    {
        string country = countryCode.ToString().ToLowerInvariant();
        return exportType switch
        {
            ExportType.Listings => $"{country}-listings",
            ExportType.Updates => $"{country}-listings-updates",
            ExportType.Published => $"{country}-listings-published",
            ExportType.Unpublished => $"{country}-listings-unpublished",
            ExportType.PublishedUpdates => $"{country}-listings-published-updates",
            ExportType.UnpublishedUpdates => $"{country}-listings-unpublished-updates",
            ExportType.Websites => $"{country}-websites",
            _ => countryCode + "_" + exportType
        };
    }

    public static string GetObjectKey(
        CountryCode countryCode,
        ExportType exportType,
        string fileExtension) =>
        countryCode + "/" + exportType + "/" +
        GetFileName(countryCode, exportType, fileExtension);

    public static string GetLegacyObjectKey(
        CountryCode countryCode,
        ExportType exportType,
        string fileExtension) =>
        countryCode + "/" + exportType + "/" +
        GetLegacyFileName(countryCode, exportType, fileExtension);

    public static string GetLegacyFileName(
        CountryCode countryCode,
        ExportType exportType,
        string fileExtension) =>
        countryCode + "_" + exportType + "." + fileExtension;

    public static string GetFileNameWithDate(
        DateOnly date,
        string prefix,
        string extension) =>
        $"{prefix}-{date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.{extension}";

    public static string GetFileNameWithDateRange(
        DateOnly dateFrom,
        DateOnly dateTo,
        string prefix,
        string extension) =>
        $"{prefix}-{dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}-to-{dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.{extension}";

    public static string GetLegacyFileNameWithDate(
        DateOnly date,
        string prefix,
        string extension) =>
        prefix + "_" + date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "." + extension;

    public static string GetListingsByOperationPropertyTypeFileName(
        CountryCode countryCode,
        Operation operation,
        PropertyType propertyType,
        ListingStatus listingStatus,
        string extension) =>
        string.Join(
            "-",
            countryCode.ToString().ToLowerInvariant(),
            "listings",
            listingStatus.ToString().ToLowerInvariant(),
            operation.ToString().ToLowerInvariant(),
            propertyType.ToString().ToLowerInvariant()) + "." + extension;

    public static string GetHostListingsFileName(
        CountryCode countryCode,
        string host,
        ListingStatus listingStatus,
        string extension) =>
        string.Join(
            "-",
            countryCode.ToString().ToLowerInvariant(),
            "listings",
            listingStatus.ToString().ToLowerInvariant(),
            host.ToLowerInvariant()) + "." + extension;
}
