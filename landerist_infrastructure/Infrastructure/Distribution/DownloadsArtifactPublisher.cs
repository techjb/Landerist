using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class DownloadsArtifactPublisher : DistributionArtifacts
{
    private readonly IDownloadsStorage _storage;
    private readonly HistoricArtifactPublisher _historic;
    private readonly DownloadUpdateDateResolver _dateResolver;

    public DownloadsArtifactPublisher(Func<DateOnly> yesterday)
        : this(
            new S3DownloadsStorage(),
            new SystemDistributionFileSystem(),
            yesterday)
    {
    }

    internal DownloadsArtifactPublisher(
        IDownloadsStorage storage,
        IDistributionFileSystem files,
        Func<DateOnly> yesterday)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(yesterday);
        _storage = storage;
        var naming = new HistoricArtifactNaming(yesterday);
        _historic = new HistoricArtifactPublisher(storage, files, naming);
        _dateResolver = new DownloadUpdateDateResolver(storage, yesterday);
    }

    public DateOnly GetDateFrom(params ExportType[] exportTypes) =>
        _dateResolver.GetDateFrom(exportTypes);

    public bool UploadListings(
        string filePath,
        CountryCode countryCode,
        ExportType exportType,
        int counter,
        DateOnly? dateFrom,
        DateOnly? dateTo) =>
        UploadWithHistoric(
            filePath,
            GetFileName(countryCode, exportType, "json"),
            GetLocalSubdirectory(countryCode, exportType),
            countryCode,
            exportType,
            counter,
            dateFrom,
            dateTo,
            "json");

    public bool UploadWebsites(
        string filePath,
        CountryCode countryCode,
        int counter) =>
        UploadWithHistoric(
            filePath,
            GetFileName(countryCode, ExportType.Websites, "csv"),
            GetLocalSubdirectory(countryCode, ExportType.Websites),
            countryCode,
            ExportType.Websites,
            counter,
            null,
            null,
            "csv");

    public bool UploadHost(
        string filePath,
        string host,
        ListingStatus listingStatus,
        int counter,
        string subdirectory) =>
        UploadCurrentAndLog(
            filePath,
            GetHostListingsFileName(
                CountryCode.ES,
                host,
                listingStatus,
                "json"),
            subdirectory,
            DownloadMetadataBuilder.Build(counter, null, null));

    public bool UploadOperationPropertyType(
        string filePath,
        Operation operation,
        PropertyType propertyType,
        ListingStatus listingStatus,
        int counter,
        string subdirectory) =>
        UploadCurrentAndLog(
            filePath,
            GetListingsByOperationPropertyTypeFileName(
                CountryCode.ES,
                operation,
                propertyType,
                listingStatus,
                "json"),
            subdirectory,
            DownloadMetadataBuilder.Build(counter, null, null));

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
        IReadOnlyList<(string Key, string Value)> metadata =
            DownloadMetadataBuilder.Build(counter, dateFrom, dateTo);
        if (!UploadCurrent(filePath, fileName, subdirectory, metadata))
        {
            return false;
        }

        bool uploaded = _historic.Upload(
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

    private bool UploadCurrentAndLog(
        string filePath,
        string fileName,
        string subdirectory,
        IReadOnlyList<(string Key, string Value)> metadata)
    {
        bool uploaded = UploadCurrent(filePath, fileName, subdirectory, metadata);
        if (uploaded)
        {
            Log.WriteInfo("filesupdater", fileName);
        }

        return uploaded;
    }

    private bool UploadCurrent(
        string filePath,
        string fileName,
        string subdirectory,
        IReadOnlyList<(string Key, string Value)> metadata) =>
        _storage.Upload(
            filePath,
            fileName,
            subdirectory,
            metadata,
            $"attachment; filename=\"{fileName}\"");
}
