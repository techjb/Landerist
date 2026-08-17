using landerist_library.Export;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsPageRenderer
{
    private readonly string _templatePath;
    private readonly string _outputPath;

    public StatisticsPageRenderer(string templatePath, string outputPath)
    {
        _templatePath = templatePath;
        _outputPath = outputPath;
    }

    public bool Render(string summaryTable, IReadOnlyList<string> charts)
    {
        string html = File.ReadAllText(_templatePath);
        html = html.Replace("/*SUMMARY_TABLE*/", summaryTable);
        html = html.Replace(
            "/*CHARTS*/",
            string.Join("; " + Environment.NewLine, charts));
        File.WriteAllText(_outputPath, html);
        return new S3().UploadToWebsiteBucket(_outputPath, "index.html", "statistics");
    }
}
