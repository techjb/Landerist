using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class HttpConditionalPageHeaderService : IConditionalPageHeaderService
{
    public ConditionalPageHeaderResult Check(Page page, bool useProxy)
    {
        var result = new ConditionalPageHeaderChecker(useProxy).Check(page);
        return new ConditionalPageHeaderResult
        {
            NotModified = result.NotModified,
            HttpStatusCode = result.HttpStatusCode,
            RedirectUrl = result.RedirectUrl,
            Etag = result.Etag,
            LastModified = result.LastModified
        };
    }
}
