namespace landerist_library.Application.Scraping;

public interface IScrapeProgressReporter
{
    void Write(string message);
}