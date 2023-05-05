using PuppeteerSharp;

namespace landerist_library.Download
{
    public class PuppeteerDownloader
    {
        private static readonly HashSet<ResourceType> BlockResources = new()
        {
            ResourceType.Image,
            ResourceType.StyleSheet,
            ResourceType.Font,
            ResourceType.Media,
            ResourceType.Other
        };

        private static readonly HashSet<string> BlockDomains = new()
        {
            "www.google-analytics.com"
        };

        public static string Get(Uri uri)
        {
            return Task.Run(async () => await GetAsync(uri)).Result;
        }

        private static async Task<string> GetAsync(Uri uri)
        {
            try
            {
                // Download chromium browser. Run only the first time.
                //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = false,
                    Devtools = true,
                });

                using var page = await browser.NewPageAsync();
                await page.SetRequestInterceptionAsync(true);
                page.Request += async (sender, e) =>
                {
                    if (BlockResources.Contains(e.Request.ResourceType))
                    {
                        await e.Request.AbortAsync();
                        return;
                    }
                    Uri uri = new(e.Request.Url);
                    if (BlockDomains.Contains(uri.Host))
                    {
                        await e.Request.AbortAsync();
                        return;
                    }
                    await e.Request.ContinueAsync();
                };

                await page.GoToAsync(uri.ToString());

                //var pageText = await page.EvaluateFunctionAsync<string>("() => document.body.innerText");
                return await page.GetContentAsync();
            }
            catch { }
            return string.Empty;
        }
    }
}
