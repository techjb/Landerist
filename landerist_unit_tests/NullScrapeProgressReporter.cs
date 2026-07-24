using landerist_library.Application.Scraping;

namespace landerist_unit_tests;

internal sealed class NullScrapeProgressReporter : IScrapeProgressReporter
{
    public void Write(string message)
    {
    }
}