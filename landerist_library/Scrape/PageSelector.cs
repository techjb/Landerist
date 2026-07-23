using landerist_library.Configuration;
using landerist_library.Application;
using landerist_library.Pages;

namespace landerist_library.Scrape
{
    /// <summary>
    /// Transitional facade for legacy callers. New code should receive
    /// IPageBatchSelector through constructor injection.
    /// </summary>
    public class PageSelector
    {
        public static List<Page> Select() =>
            [.. LanderistApplication.Services.PageBatchSelector.Select()];
    }
}
