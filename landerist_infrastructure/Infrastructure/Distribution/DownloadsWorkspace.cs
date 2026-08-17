using landerist_library.Export;
using landerist_library.Logs;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class DownloadsWorkspace : DistributionArtifacts
{
    public string EnsureSubdirectory(string subdirectory)
    {
        string directory = GetFilePath(subdirectory);
        Directory.CreateDirectory(directory);
        return directory;
    }

    public string GetArtifactPath(string subdirectory, string fileName)
    {
        EnsureSubdirectory(subdirectory);
        return GetFilePath(subdirectory, fileName);
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
