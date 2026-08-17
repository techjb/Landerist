using System.Globalization;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class DownloadUpdateDateResolver
{
    private readonly IDownloadsStorage _storage;
    private readonly Func<DateOnly> _yesterday;

    public DownloadUpdateDateResolver(
        IDownloadsStorage storage,
        Func<DateOnly> yesterday)
    {
        _storage = storage;
        _yesterday = yesterday;
    }

    public DateOnly GetDateFrom(params ExportType[] exportTypes)
    {
        DateOnly dateFrom = _yesterday();
        foreach (ExportType exportType in exportTypes)
        {
            foreach (string objectKey in HistoricArtifactNaming.GetMetadataObjectKeys(exportType))
            {
                string? value = _storage.GetMetadata(
                    objectKey,
                    DownloadsUpdater.METADATA_KEY_DATETO);
                if (value is not null &&
                    DateOnly.TryParseExact(
                        value,
                        DownloadMetadataBuilder.DateFormat,
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
}
