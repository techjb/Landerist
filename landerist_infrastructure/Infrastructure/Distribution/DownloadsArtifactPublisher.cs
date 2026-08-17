using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class DownloadsArtifactPublisher : DistributionArtifacts
{
    private const string MetadataDateFormat = "yyyy-MM-dd";
    private readonly Func<DateOnly> _yesterday;

    public DownloadsArtifactPublisher(Func<DateOnly> yesterday) =>
        _yesterday = yesterday;

    public DateOnly GetDateFrom(params ExportType[] exportTypes)
    {
        DateOnly dateFrom = _yesterday();
        var s3 = new S3();

        foreach (ExportType exportType in exportTypes)
        {
            foreach (string objectKey in GetObjectKeysForMetadata(exportType))
            {
                string? metadataValue = s3.GetMetadataValue(
                    AppConfig.AWS_S3_DOWNLOADS_BUCKET,
                    objectKey,
                    DownloadsUpdater.METADATA_KEY_DATETO);
                if (metadataValue is not null &&
                    DateOnly.TryParseExact(
                        metadataValue,
                        MetadataDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateOnly dateTo))
                {
                    if (dateTo.AddDays(1) < dateFrom)
                    {
                        dateFrom = dateTo.AddDays(1);
                    }

                    break;
                }
            }
        }

        return dateFrom;
    }

    public bool UploadListings(
        string filePath,
        CountryCode countryCode,
        ExportType exportType,
        int counter,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        string subdirectory = GetLocalSubdirectory(countryCode, exportType);
        string fileName = GetFileName(countryCode, exportType, "json");
        return UploadWithHistoric(
            filePath,
            fileName,
            subdirectory,
            countryCode,
            exportType,
            counter,
            dateFrom,
            dateTo,
            "json");
    }

    public bool UploadWebsites(
        string filePath,
        CountryCode countryCode,
        int counter)
    {
        const ExportType exportType = ExportType.Websites;
        string subdirectory = GetLocalSubdirectory(countryCode, exportType);
        string fileName = GetFileName(countryCode, exportType, "csv");
        return UploadWithHistoric(
            filePath,
            fileName,
            subdirectory,
            countryCode,
            exportType,
            counter,
            null,
            null,
            "csv");
    }

    public bool UploadHost(
        string filePath,
        string host,
        ListingStatus listingStatus,
        int counter,
        string subdirectory)
    {
        string fileName = GetHostListingsFileName(
            CountryCode.ES,
            host,
            listingStatus,
            "json");
        bool uploaded = UploadCurrent(
            filePath,
            fileName,
            subdirectory,
            GetMetadata(counter, null, null),
            contentDisposition: true);
        if (uploaded)
        {
            Log.WriteInfo("filesupdater", fileName);
        }

        return uploaded;
    }

    public bool UploadOperationPropertyType(
        string filePath,
        Operation operation,
        PropertyType propertyType,
        ListingStatus listingStatus,
        int counter,
        string subdirectory)
    {
        string fileName = GetListingsByOperationPropertyTypeFileName(
            CountryCode.ES,
            operation,
            propertyType,
            listingStatus,
            "json");
        bool uploaded = UploadCurrent(
            filePath,
            fileName,
            subdirectory,
            GetMetadata(counter, null, null),
            contentDisposition: true);
        if (uploaded)
        {
            Log.WriteInfo("filesupdater", fileName);
        }

        return uploaded;
    }

    private bool UploadWithHistoric(
        string filePath,
        string fileName,
        string subdirectory,
        CountryCode countryCode,
        ExportType exportType,
        int counter,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string extension)
    {
        List<(string, string)> metadata = GetMetadata(counter, dateFrom, dateTo);
        if (!UploadCurrent(
            filePath,
            fileName,
            subdirectory,
            metadata,
            contentDisposition: true))
        {
            return false;
        }

        bool uploaded = UploadHistoric(
            countryCode,
            exportType,
            filePath,
            subdirectory,
            metadata,
            dateFrom,
            dateTo,
            extension);
        if (uploaded)
        {
            Log.WriteInfo("filesupdater", fileName);
        }

        return uploaded;
    }

    private static bool UploadCurrent(
        string filePath,
        string fileName,
        string subdirectory,
        List<(string, string)> metadata,
        bool contentDisposition)
    {
        string bucketSubdirectory = subdirectory.Replace("\\", "/");
        string? disposition = contentDisposition
            ? $"attachment; filename=\"{fileName}\""
            : null;
        if (!new S3().UploadToDownloadsBucket(
            filePath,
            fileName,
            bucketSubdirectory,
            metadata,
            disposition))
        {
            return false;
        }

        return true;
    }

    private bool UploadHistoric(
        CountryCode countryCode,
        ExportType exportType,
        string filePath,
        string subdirectory,
        List<(string, string)> metadata,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string extension)
    {
        string fileName = GetHistoricFileName(
            countryCode,
            exportType,
            dateFrom,
            dateTo,
            extension);
        string localSubdirectory = GetLocalSubdirectory(countryCode, exportType);
        string historicPath = GetFilePath(localSubdirectory, fileName);

        try
        {
            File.Copy(filePath, historicPath, true);
            return UploadCurrent(
                historicPath,
                fileName,
                subdirectory,
                metadata,
                contentDisposition: true);
        }
        finally
        {
            if (File.Exists(historicPath))
            {
                File.Delete(historicPath);
            }
        }
    }

    private string GetHistoricFileName(
        CountryCode countryCode,
        ExportType exportType,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string extension)
    {
        string prefix = GetFileName(countryCode, exportType);
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

        return UsesModernHistoricFileName(exportType)
            ? GetFileNameWithDate(_yesterday(), prefix, extension)
            : GetLegacyFileNameWithDate(_yesterday(), prefix, extension);
    }

    private static bool UsesModernHistoricFileName(ExportType exportType) =>
        exportType is ExportType.Published
            or ExportType.Unpublished
            or ExportType.PublishedUpdates
            or ExportType.UnpublishedUpdates
            or ExportType.Websites;

    private static List<(string, string)> GetMetadata(
        int counter,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        List<(string, string)> metadata =
        [
            (DownloadsUpdater.METADATA_KEY_COUNTER,
                counter.ToString(CultureInfo.InvariantCulture))
        ];

        if (dateFrom.HasValue)
        {
            metadata.Add((
                DownloadsUpdater.METADATA_KEY_DATEFROM,
                dateFrom.Value.ToString(MetadataDateFormat, CultureInfo.InvariantCulture)));
        }

        if (dateTo.HasValue)
        {
            metadata.Add((
                DownloadsUpdater.METADATA_KEY_DATETO,
                dateTo.Value.ToString(MetadataDateFormat, CultureInfo.InvariantCulture)));
        }

        return metadata;
    }

    private static List<string> GetObjectKeysForMetadata(ExportType exportType) =>
        new List<string>
        {
            GetObjectKey(CountryCode.ES, exportType, "json"),
            GetObjectKey(CountryCode.ES, exportType, "zip"),
            GetLegacyObjectKey(CountryCode.ES, exportType, "json"),
            GetLegacyObjectKey(CountryCode.ES, exportType, "zip")
        }
        .Distinct(StringComparer.Ordinal)
        .ToList();
}
