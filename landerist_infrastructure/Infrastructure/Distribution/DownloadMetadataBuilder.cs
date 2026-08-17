using System.Globalization;

namespace landerist_library.Infrastructure.Distribution;

internal static class DownloadMetadataBuilder
{
    public const string DateFormat = "yyyy-MM-dd";

    public static IReadOnlyList<(string Key, string Value)> Build(
        int counter,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        List<(string Key, string Value)> metadata =
        [
            (DownloadsUpdater.METADATA_KEY_COUNTER,
                counter.ToString(CultureInfo.InvariantCulture))
        ];
        if (dateFrom.HasValue)
        {
            metadata.Add((
                DownloadsUpdater.METADATA_KEY_DATEFROM,
                dateFrom.Value.ToString(DateFormat, CultureInfo.InvariantCulture)));
        }

        if (dateTo.HasValue)
        {
            metadata.Add((
                DownloadsUpdater.METADATA_KEY_DATETO,
                dateTo.Value.ToString(DateFormat, CultureInfo.InvariantCulture)));
        }

        return metadata;
    }
}
