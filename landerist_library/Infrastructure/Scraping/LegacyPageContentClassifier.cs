using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Parse.PageTypeParser;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyPageContentClassifier : IPageContentClassifier
{
    public PageClassificationResult Classify(Page page)
    {
        var result = new PageTypeParser(page).GetPageType();
        return new PageClassificationResult(
            result.pageType,
            result.listing,
            result.waitingAIRequest);
    }
}
