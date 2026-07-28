using landerist_library.Websites;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Http;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class HttpConditionalPageHeaderService(IHttpClientTransportFactory httpClients) : IConditionalPageHeaderService
{
    private readonly IHttpClientTransportFactory _httpClients =
        httpClients ?? throw new ArgumentNullException(nameof(httpClients));
    public ConditionalPageHeaderResult Check(Page page, bool useProxy)
    {
        var result = new ConditionalPageHeaderChecker(useProxy, _httpClients).Check(page);
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
