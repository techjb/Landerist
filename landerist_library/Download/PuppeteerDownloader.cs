using HtmlAgilityPack;
using PuppeteerSharp;
using landerist_library.Configuration;
using System;


namespace landerist_library.Download
{
    public class PuppeteerDownloader
    {
        private short? Status = null;

        private string? RedirectUrl = null;

        private string? Html = null;

        private readonly HashSet<ResourceType> BlockResources =
        [
            ResourceType.Image,
            ResourceType.ImageSet,
            ResourceType.Img,
            //ResourceType.StyleSheet,
            ResourceType.Font,
            ResourceType.Media,            
            //ResourceType.Other
        ];

        private static readonly LaunchOptions launchOptions = new()
        {
            Headless = Config.IsConfigurationProduction(),
            Devtools = false,
            Args = new string[] {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-position=0,0",
                "--ignore-certifcate-errors",
                "--ignore-certifcate-errors-spki-list"
            },
        };

        private static readonly HashSet<string> BlockDomains =
        [
            "www.google-analytics.com"
        ];

        public string? GetText(Uri uri)
        {
            Html = Get(uri);
            if (Html != null)
            {
                HtmlDocument htmlDocument = new();
                try
                {
                    htmlDocument.LoadHtml(Html);
                    return Tools.HtmlToText.GetText(htmlDocument);
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteLogErrors("PuppeteerDownloader GetText", exception);
                }
            }
            return null;
        }

        public string? Get(Uri uri)
        {
            return Task.Run(async () => await GetAsync(uri)).Result;
        }

        private async Task<string?> GetAsync(Uri uri)
        {
            try
            {
                // Download chromium browser. Run only the first time.
                await new BrowserFetcher().DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
                await page.SetUserAgentAsync(Config.USER_AGENT);
                await page.SetRequestInterceptionAsync(true);
                page.Request += async (sender, e) => await HandleRequestAsync(e, uri);
                page.Response += (sender, e) => HandleResponseAsync(e, uri);

                await page.GoToAsync(uri.ToString(), WaitUntilNavigation.Networkidle0);
                return await page.GetContentAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("PuppeteerDownloader GetAsync", exception);
            }
            return null;
        }

        private async Task HandleRequestAsync(RequestEventArgs e, Uri uri)
        {
            if (BlockResources.Contains(e.Request.ResourceType))
            {
                await e.Request.AbortAsync();
                return;
            }
            if (BlockDomains.Contains(uri.Host))
            {
                await e.Request.AbortAsync();
                return;
            }
            if (e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0)
            {
                await e.Request.AbortAsync();
                return;
            }
            await e.Request.ContinueAsync();
        }

        private void HandleResponseAsync(ResponseCreatedEventArgs e, Uri uri)
        {
            if (!Uri.TryCreate(e.Response.Url, UriKind.RelativeOrAbsolute, out Uri? requestedUri))
            {
                return;
            }
            if (!requestedUri.Equals(uri))
            {
                return;
            }
            Status = (short)e.Response.Status;
            if (Status >= 300 && Status < 400)
            {
                if (e.Response.Headers.TryGetValue("Location", out string? location))
                {
                    RedirectUrl = location;
                }
            }
        }
    }
}
