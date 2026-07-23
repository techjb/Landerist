using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageClassificationMetrics
{
    void RecordPageNotModified(Page page);
    void RecordNotListingCache(Page page);
    void RecordListingInputAlreadyParsed(Page page);
}
