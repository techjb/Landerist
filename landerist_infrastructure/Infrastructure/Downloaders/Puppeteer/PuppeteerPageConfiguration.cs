using landerist_library.Websites;
using PuppeteerSharp;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer;

internal static class PuppeteerPageConfiguration
{
    internal static async Task ConfigureAsync(
        IPage browserPage,
        Website website,
        int navigationTimeout,
        EventHandler<RequestEventArgs> requestHandler,
        EventHandler<ResponseCreatedEventArgs> responseHandler)
    {
        browserPage.DefaultNavigationTimeout = navigationTimeout;
        await SetExtraHttpHeadersAsync(browserPage, website).ConfigureAwait(false);
        await browserPage.SetUserAgentAsync(website.BrowserUserAgent).ConfigureAwait(false);
        await browserPage.SetCacheEnabledAsync(false).ConfigureAwait(false);
        await browserPage.SetRequestInterceptionAsync(true).ConfigureAwait(false);
        browserPage.Request += requestHandler;
        browserPage.Response += responseHandler;
    }

    private static async Task SetExtraHttpHeadersAsync(IPage browserPage, Website website)
    {
        Dictionary<string, string> extraHeaders = new(
            WebsiteHttpRequestProfile.From(website).Headers,
            StringComparer.OrdinalIgnoreCase);
        extraHeaders.Remove("User-Agent");
        if (website.LanguageCode == LanguageCode.es)
        {
            extraHeaders.TryAdd("Accept-Language", "es-ES, es;q=0.9");
        }

        if (extraHeaders.Count > 0)
        {
            await browserPage.SetExtraHttpHeadersAsync(extraHeaders).ConfigureAwait(false);
        }
    }
}
