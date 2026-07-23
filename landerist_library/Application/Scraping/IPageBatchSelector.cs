using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageBatchSelector
{
    IReadOnlyList<Page> Select();
}
