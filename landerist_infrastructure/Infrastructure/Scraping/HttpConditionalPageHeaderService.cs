using landerist_library.Websites;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Http;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class HttpConditionalPageHeaderService(IHttpClientTransportFactory httpClients) : IConditionalPageHeaderService
{
    private readonly IHttpClientTransportFactory _httpClients =
        httpClients ?? throw new ArgumentNullException(nameof(httpClients));
    public ConditionalPageHeaderResult Check(Page page, bool useProxy) =>
        Map(new ConditionalPageHeaderChecker(useProxy, _httpClients).Check(page));

    public async Task<ConditionalPageHeaderResult> CheckAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken = default)
    {
        ConditionalHeaderCheckResult result = await new ConditionalPageHeaderChecker(
            useProxy,
            _httpClients).CheckAsync(page, cancellationToken).ConfigureAwait(false);
        return Map(result);
    }

    private static ConditionalPageHeaderResult Map(ConditionalHeaderCheckResult result) =>
        new()
        {
            NotModified = result.NotModified,
            HttpStatusCode = result.HttpStatusCode,
            RedirectUrl = result.RedirectUrl,
            Etag = result.Etag,
            LastModified = result.LastModified
        };
}
