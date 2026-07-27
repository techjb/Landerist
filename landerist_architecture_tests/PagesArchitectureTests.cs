using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed class PagesArchitectureTests
{
    [Fact]
    public void Pages_DoesNotDependOnGlobalConfiguration()
    {
        string pagesRoot = Path.Combine(FindRepositoryRoot(), "landerist_library", "Pages");
        string[] violations = Directory
            .EnumerateFiles(pagesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return source.Contains("landerist_library.Configuration", StringComparison.Ordinal) ||
                    Regex.IsMatch(source, @"\bConfig\b", RegexOptions.CultureInvariant);
            })
            .Select(file => Path.GetRelativePath(FindRepositoryRoot(), file).Replace('\\', '/'))
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Pages must receive policies explicitly instead of depending on Configuration or Config." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Pages_DoesNotDependOnDownloaderImplementations()
    {
        string pagesRoot = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Pages");
        string[] violations = Directory
            .EnumerateFiles(pagesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains(
                "landerist_library.Downloaders",
                StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file)!)
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Pages must receive domain download results instead of downloader implementations." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Pages_DoesNotWriteToGlobalLogs()
    {
        string pagesRoot = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Pages");
        string[] violations = Directory
            .EnumerateFiles(pagesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains(
                "Logs.Log",
                StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file)!)
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Pages must report failures through return values instead of global logging." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
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
