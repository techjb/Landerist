namespace landerist_architecture_tests;

public sealed class DownloadsDistributionArchitectureTests
{
    [Fact]
    public void DownloadsUpdater_IsOnlyTheDistributionCoordinator()
    {
        string updater = ReadDistributionSource("DownloadsUpdater.cs");

        Assert.Contains("DownloadsWorkspace", updater);
        Assert.Contains("DownloadsArtifactPublisher", updater);
        Assert.Contains("SegmentedListingsUpdater", updater);
        Assert.DoesNotContain("new S3", updater);
        Assert.DoesNotContain("File.", updater);
        Assert.DoesNotContain("Directory.", updater);
        Assert.DoesNotContain("Json.ExportListings", updater);
        Assert.DoesNotContain("Tools.Csv", updater);
    }

    [Fact]
    public void DistributionSideEffects_HaveFocusedOwners()
    {
        string workspace = ReadDistributionSource("DownloadsWorkspace.cs");
        string publisher = ReadDistributionSource("DownloadsArtifactPublisher.cs");
        string storage = ReadDistributionSource("S3DownloadsStorage.cs");
        string segmented = ReadDistributionSource("SegmentedListingsUpdater.cs");

        Assert.Contains("Json.ExportListings", workspace);
        Assert.Contains("Tools.Csv.Write", workspace);
        Assert.DoesNotContain("landerist_library.Export", publisher);
        Assert.DoesNotContain("File.", publisher);
        Assert.Contains("private readonly S3", storage);
        Assert.Contains("UploadToDownloadsBucket", storage);
        Assert.DoesNotContain("new S3", workspace);
        Assert.DoesNotContain("new S3", segmented);
    }

    private static string ReadDistributionSource(string fileName)
    {
        string root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Distribution",
            fileName));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Landerist.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing Landerist.sln.");
    }
}
