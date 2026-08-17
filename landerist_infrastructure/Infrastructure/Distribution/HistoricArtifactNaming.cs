using landerist_library.Websites;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class HistoricArtifactNaming : DistributionArtifacts
{
    private readonly Func<DateOnly> _yesterday;

    public HistoricArtifactNaming(Func<DateOnly> yesterday) =>
        _yesterday = yesterday;

    public string GetFileName(
        CountryCode countryCode,
        ExportType exportType,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string extension)
    {
        string prefix = DistributionArtifacts.GetFileName(countryCode, exportType);
        if (exportType is ExportType.PublishedUpdates or ExportType.UnpublishedUpdates &&
            dateFrom.HasValue &&
            dateTo.HasValue)
        {
            return GetFileNameWithDateRange(
                dateFrom.Value,
                dateTo.Value,
                prefix,
                extension);
        }

        return UsesModernName(exportType)
            ? GetFileNameWithDate(_yesterday(), prefix, extension)
            : GetLegacyFileNameWithDate(_yesterday(), prefix, extension);
    }

    public static IReadOnlyList<string> GetMetadataObjectKeys(ExportType exportType) =>
        new List<string>
        {
            GetObjectKey(CountryCode.ES, exportType, "json"),
            GetObjectKey(CountryCode.ES, exportType, "zip"),
            GetLegacyObjectKey(CountryCode.ES, exportType, "json"),
            GetLegacyObjectKey(CountryCode.ES, exportType, "zip")
        }
        .Distinct(StringComparer.Ordinal)
        .ToList();

    private static bool UsesModernName(ExportType exportType) =>
        exportType is ExportType.Published
            or ExportType.Unpublished
            or ExportType.PublishedUpdates
            or ExportType.UnpublishedUpdates
            or ExportType.Websites;
}
