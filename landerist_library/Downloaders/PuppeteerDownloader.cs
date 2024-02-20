using HtmlAgilityPack;
using PuppeteerSharp;
using landerist_library.Configuration;
using landerist_library.Websites;


namespace landerist_library.Downloaders
{
    public class PuppeteerDownloader : IDownloader
    {
        private short? HttpStatusCode = null;

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
            //ResourceType.WebSocket,            
            //ResourceType.Other
        ];

        private static readonly LaunchOptions launchOptions = new()
        {
            Headless = true, // if false, need to comment await browserPage.SetRequestInterceptionAsync(true);
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
            "www.google-analytics.com",
            "www.googletagmanager.com",
            "tagmanager.google.com",
            "doubleclick.net",
            "connect.facebook.net",
            "stats.g.doubleclick.net",
            "adservice.google.com",
            "pagead2.googlesyndication.com",
            "mads.amazon-adsystem.com",
            "ad.doubleclick.net",
            "maps.googleapis.com",
        ];

        public void DoTest()
        {
            var page = new Websites.Page("http://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");
            //var page = new Websites.Page("https://www.realestate.bnpparibas.es/es/soluciones-medida/soluciones-para-inversores");
            var text = new PuppeteerDownloader().GetText(page);
            if (text != null)
            {
                Console.WriteLine(text);
                Logs.Log.WriteLogInfo("PuppeterTest", text);
            }
            else
            {
                Logs.Log.WriteLogInfo("PuppeterTest", "Text is null");
            }

        }

        public string? GetText(Websites.Page page)
        {
            var html = GetResponseBody(page);
            if (html != null)
            {
                HtmlDocument htmlDocument = new();
                try
                {
                    htmlDocument.LoadHtml(html);
                    return Tools.HtmlToText.GetText(htmlDocument);
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteLogErrors("PuppeteerDownloader GetText", exception);
                }
            }
            return null;
        }
        
        public void SetResponseBodyAndStatusCode(Websites.Page page)
        {
            page.InitializeResponseBodyAndStatusCode();
            var responseBody = GetResponseBody(page);
            page.SetResponseBodyAndStatusCode(responseBody, HttpStatusCode);
        }

        public string? GetResponseBody(Websites.Page page)
        {
            try
            {
                Html = Task.Run(async () => await GetAsync(page.Website.LanguageCode, page.Uri)).Result;
            }
            catch(Exception exception) 
            {
                Logs.Log.WriteLogErrors("PuppeterDownloader GetResponseBody", exception);
            }
            return Html;
        }


        private async Task<string?> GetAsync(LanguageCode languageCode, Uri uri)
        {
            //await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(launchOptions);            
            try
            {
                var browserPage = await GetBroserPage(browser, languageCode, uri);
                await browserPage.GoToAsync(uri.ToString(), WaitUntilNavigation.Networkidle0);
                return await browserPage.GetContentAsync();
            }
            catch
            {
                await browser.CloseAsync();
                browser.Dispose();
            }
            return null;
        }

        private async Task<IPage> GetBroserPage(IBrowser browser, LanguageCode languageCode, Uri uri)
        {
            var browserPage = await browser.NewPageAsync();
            browserPage.DefaultNavigationTimeout = Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
            SetAccepLanguage(browserPage, languageCode);
            await browserPage.SetUserAgentAsync(Config.USER_AGENT);
            await browserPage.SetRequestInterceptionAsync(true);
            browserPage.Request += async (sender, e) => await HandleRequestAsync(e, uri);
            browserPage.Response += (sender, e) => HandleResponseAsync(e, uri);
            return browserPage;
        }

        private void SetAccepLanguage(IPage browserPage, Websites.LanguageCode languageCode)
        {
            Dictionary<string, string> extraHeaders = [];
            switch (languageCode)
            {
                case LanguageCode.es:
                    {
                        extraHeaders.Add("Accept-Language", "es-ES, es;q=0.9");
                    }
                    break;
            }
            if (extraHeaders.Count > 0)
            {
                browserPage.SetExtraHttpHeadersAsync(extraHeaders);
            }
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
            HttpStatusCode = (short)e.Response.Status;
            if (e.Response.Headers.TryGetValue("Location", out string? location))
            {
                RedirectUrl = location;
            }
        }

        public string? GetRedirectUrl()
        {
            return RedirectUrl;
        }
    }
}
