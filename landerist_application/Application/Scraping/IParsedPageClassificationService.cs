using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping;

public interface IParsedPageClassificationService
{
    bool Apply(Page page, PageType pageType, Listing? listing);
}
