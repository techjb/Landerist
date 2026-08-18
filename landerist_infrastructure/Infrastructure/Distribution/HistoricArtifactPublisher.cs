using landerist_library.Websites;
using static landerist_library.Infrastructure.Distribution.DistributionArtifactNaming;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class HistoricArtifactPublisher
{
    private readonly IDownloadsStorage _storage;
    private readonly IDistributionFileSystem _files;
    private readonly HistoricArtifactNaming _naming;
    private readonly string _exportDirectory;

    public HistoricArtifactPublisher(
        IDownloadsStorage storage,
        IDistributionFileSystem files,
        HistoricArtifactNaming naming,
        string exportDirectory)
    {
        _storage = storage;
        _files = files;
        _naming = naming;
        _exportDirectory = exportDirectory;
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
        string historicPath = Path.Combine(
            _exportDirectory,
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
