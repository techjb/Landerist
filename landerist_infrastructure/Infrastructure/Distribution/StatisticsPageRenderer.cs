using landerist_library.Export;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsPageRenderer
{
    private readonly string _templatePath;
    private readonly string _outputPath;
    private readonly IWebsiteArtifactStorage _storage;
    private readonly IDistributionFileSystem _files;

    public StatisticsPageRenderer(
        string templatePath,
        string outputPath,
        IWebsiteArtifactStorage storage,
        IDistributionFileSystem files)
    {
        _templatePath = templatePath;
        _outputPath = outputPath;
        _storage = storage;
        _files = files;
    }

    public bool Render(string summaryTable, IReadOnlyList<string> charts)
    {
        string html = _files.ReadAllText(_templatePath);
        html = html.Replace("/*SUMMARY_TABLE*/", summaryTable);
        html = html.Replace(
            "/*CHARTS*/",
            string.Join("; " + Environment.NewLine, charts));
        _files.WriteAllText(_outputPath, html);
        return _storage.Upload(_outputPath, "index.html", "statistics");
    }
}
