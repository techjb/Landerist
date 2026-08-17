using landerist_library.Websites;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class HistoricArtifactPublisher : DistributionArtifacts
{
    private readonly IDownloadsStorage _storage;
    private readonly IDistributionFileSystem _files;
    private readonly HistoricArtifactNaming _naming;

    public HistoricArtifactPublisher(
        IDownloadsStorage storage,
        IDistributionFileSystem files,
        HistoricArtifactNaming naming)
    {
        _storage = storage;
        _files = files;
        _naming = naming;
    }

    public bool Upload(
        CountryCode countryCode,
        ExportType exportType,
        string sourcePath,
        string subdirectory,
        IReadOnlyList<(string Key, string Value)> metadata,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string extension)
    {
        string fileName = _naming.GetFileName(
            countryCode,
            exportType,
            dateFrom,
            dateTo,
            extension);
        string historicPath = GetFilePath(
            GetLocalSubdirectory(countryCode, exportType),
            fileName);
        try
        {
            _files.Copy(sourcePath, historicPath, overwrite: true);
            return _storage.Upload(
                historicPath,
                fileName,
                subdirectory,
                metadata,
                ContentDisposition(fileName));
        }
        finally
        {
            if (_files.Exists(historicPath))
            {
                _files.Delete(historicPath);
            }
        }
    }

    private static string ContentDisposition(string fileName) =>
        $"attachment; filename=\"{fileName}\"";
}
