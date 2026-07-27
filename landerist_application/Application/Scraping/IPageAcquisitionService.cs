using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageAcquisitionService
{
    PageAcquisitionStatus Acquire(Page page, bool useProxy);
}
