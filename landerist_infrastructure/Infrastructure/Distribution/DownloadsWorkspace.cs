using landerist_library.Export;
using landerist_library.Logs;
using landerist_orels.ES;
using System.Data;
using landerist_library.Application.Distribution;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class DownloadsWorkspace
{
    private readonly IDistributionFileSystem _files;
    private readonly string _exportDirectory;

    public DownloadsWorkspace(
        DistributionOptions options,
        IDistributionFileSystem files)
    {
        _files = files;
        _exportDirectory = options.ExportDirectory;
    }

    public string EnsureSubdirectory(string subdirectory)
    {
        string directory = Path.Combine(_exportDirectory, subdirectory);
        _files.CreateDirectory(directory);
        return directory;
    }

    public string GetArtifactPath(string subdirectory, string fileName)
    {
        EnsureSubdirectory(subdirectory);
        return Path.Combine(_exportDirectory, subdirectory, fileName);
    }

    public bool WriteListings(
        SortedSet<Listing> listings,
        string subdirectory,
        string fileName) =>
        Json.ExportListings(listings, GetArtifactPath(subdirectory, fileName));

    public bool WriteWebsites(DataTable websites, string subdirectory, string fileName)
    {
        string filePath = GetArtifactPath(subdirectory, fileName);
        try
        {
            Tools.Csv.Write(websites, filePath, true);
            return true;
        }
        catch (Exception exception)
        {
            Log.WriteError("filesupdater", "Error writing Websites CSV file", exception);
            return false;
        }
    }
}
