using landerist_library.Export;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsPageRenderer
{
    private readonly string _templatePath;
    private readonly string _outputPath;
    private readonly IWebsiteArtifactStorage _storage;

    public StatisticsPageRenderer(
        string templatePath,
        string outputPath,
        IWebsiteArtifactStorage storage)
    {
        _templatePath = templatePath;
        _outputPath = outputPath;
        _storage = storage;
    }

    public bool Render(string summaryTable, IReadOnlyList<string> charts)
    {
        string html = File.ReadAllText(_templatePath);
        html = html.Replace("/*SUMMARY_TABLE*/", summaryTable);
        html = html.Replace(
            "/*CHARTS*/",
            string.Join("; " + Environment.NewLine, charts));
        File.WriteAllText(_outputPath, html);
        return _storage.Upload(_outputPath, "index.html", "statistics");
    }
}
