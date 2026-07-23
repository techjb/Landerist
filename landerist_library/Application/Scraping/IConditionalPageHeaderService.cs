using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IConditionalPageHeaderService
{
    ConditionalPageHeaderResult Check(Page page, bool useProxy);
}
