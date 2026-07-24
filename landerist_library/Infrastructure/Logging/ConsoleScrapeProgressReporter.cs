using landerist_library.Application.Scraping;

namespace landerist_library.Infrastructure.Logging;

public sealed class ConsoleScrapeProgressReporter : IScrapeProgressReporter
{
    public void Write(string message) => Console.WriteLine(message);
}