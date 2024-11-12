using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Websites;
using PuppeteerSharp;
using System.Diagnostics;


namespace landerist_library.Downloaders.Puppeteer
{
    public class PuppeteerDownloader : IDownloader
    {
        public short? HttpStatusCode { get; set; } = null;
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;

        private const string ExpressionRemoveCookies =
            @"document.querySelectorAll('[class*=""cookie"" i], [id*=""cookie"" i]').forEach(el => el.remove());";

        private static readonly HashSet<ResourceType> BlockResources = Config.TAKE_SCREENSHOT ?
        [
            ResourceType.Font,
            ResourceType.Media,
        ] :
        [
            ResourceType.Image,
            ResourceType.ImageSet,
            ResourceType.Img,
            ResourceType.Font,
            ResourceType.Media,
        ];

        private static readonly string IDontCareAboutCookies = Config.CHROME_EXTENSIONS_DIRECTORY
            + "IDontCareAboutCookies\\1.0.1_0\\";


        private static readonly string[] LaunchOptionsArgs =
            [
                "--no-sandbox",
                "--disable-notifications",
                "--disable-infobars",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--disable-features=TranslateUI",
                "--disable-features=ChromeLabs",
                "--window-position=0,0",
                "--ignore-certificate-errors",
                "--ignore-certificate-errors-spki-list",
            ];

        private static readonly string[] LaunchOptionsScreenShot =
            [
                "--disable-extensions-except=" + IDontCareAboutCookies,
                "--load-extension=" + IDontCareAboutCookies
            ];

        private static readonly LaunchOptions launchOptions = new()
        {
            //Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);          
            Headless = Config.IsConfigurationProduction(),
            Devtools = false,
            //IgnoreHTTPSErrors = true,            
            Args = Config.TAKE_SCREENSHOT ? [.. LaunchOptionsArgs, .. LaunchOptionsScreenShot] : LaunchOptionsArgs,
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
            "ads.yahoo.com",
            "ads.twitter.com",
            "analytics.twitter.com",
            "cdn.taboola.com",
            "ads.pubmatic.com",
            "adsymptotic.com",
            "pixel.quantserve.com",
            "googleads.g.doubleclick.net",
            "adroll.com",
            "media.net",
            "scorecardresearch.com",
            "ssl.google-analytics.com",
            "tracking.kissmetrics.com",
            "banners.adfox.ru",
            "static.criteo.net",
            "ib.adnxs.com",
            "cdn.adsafeprotected.com",
            "contextweb.com",
            "onetag.io",
            "rubiconproject.com",
            "yieldmo.com",
            "casalemedia.com",
            "googlesyndication.com",
            "adsafeprotected.com",
            "moatads.com",
            "criteo.com",
            "openx.net",
            "yahoo.com",
            "cloudflareinsights.com",
            "adlightning.com",
            "advertising.com",
            "zqtk.net",
            "everesttech.net",
            "demdex.net",
            "gumgum.com",
            "outbrain.com",
            "bing.com",
            "pippio.com"
        ];

        private readonly IBrowser? Browser;        

        private static readonly NavigationOptions NavigationOptions = new()
        {
            WaitUntil = [WaitUntilNavigation.Networkidle0],
            Timeout = GetTimeout(),
        };

        public PuppeteerDownloader()
        {
            Browser = Task.Run(LaunchAsync).Result;
        }

        public bool BrowserInitialized()
        {
            return Browser != null;
        }

        private static async Task<IBrowser?> LaunchAsync()
        {
            try
            {
                return await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader LaunchAsync", exception);
            }
            return null;
        }

        public void CloseBrowser()
        {
            Task.Run(CloseBrowserAsync);
        }

        private async Task CloseBrowserAsync()
        {
            try
            {
                if (BrowserInitialized())
                {
                    await Browser!.CloseAsync();
                    Browser.Dispose();
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader CloseBrowserAsync", exception);
            }
        }

        public static void UpdateChromeAndDoTest()
        {
            UpdateChrome();
            DoTest();
        }

        public static void UpdateChrome()
        {
            bool sucess = Task.Run(DownloadBrowserAsync).Result;
            Logs.Log.WriteInfo("service", "Updating Chrome. Success: " + sucess);
        }

        private static async Task<bool> DownloadBrowserAsync()
        {
            try
            {
                BrowserFetcher browserFetcher = new();
                await browserFetcher.DownloadAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void KillChrome()
        {
            if (!Config.IsConfigurationProduction())
            {
                return;
            }
            Process[] processes = Process.GetProcessesByName("chrome");
            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteError("PuppeteerDownloader", exception);
                }
            }
        }

        private void CloseAllPagesExceptFirst()
        {
            if (Browser == null)
            {
                return;
            }
            try
            {
                var pages = Task.Run(async () => await Browser.PagesAsync()).Result;
                for (var i = 0; i < pages.Length - 1; i++)
                {
                    var page = pages[i];
                    page.CloseAsync();
                }

            }
            catch// (Exception exception)
            {
                //Logs.Log.WriteError("PuppeteerDownloader CloseAllPagesExceptFirst", exception);
            }
        }

        public static void DoTest()
        {
            // working
            Websites.Page page = new("https://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");

            // http - > https
            //var page = new Websites.Page("http://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");
            //var page = new Websites.Page("http://www.inmogyb.com/es/buscador/alquiler/es/buscador/alquiler/trastero");

            // redirect example: has to throw error
            //var page = new Websites.Page("https://www.realestate.bnpparibas.es/es/soluciones-medida/soluciones-para-inversores");


            Logs.Log.WriteInfo("PuppeteerTest", "Starting test");
            string? text = new PuppeteerDownloader().GetText(page);

            Console.WriteLine(text);
            Logs.Log.WriteInfo("PuppeteerTest", "Result: " + text);
        }

        public string? GetText(Websites.Page page)
        {
            SetContentAndScrenshot(page);
            if (Content != null)
            {
                HtmlDocument htmlDocument = new();
                try
                {
                    htmlDocument.LoadHtml(Content);
                    return Tools.HtmlToText.GetText(htmlDocument);
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteError("PuppeteerDownloader GetText", exception);
                }
            }
            return null;
        }


        public void Download(Websites.Page page)
        {
            SetContentAndScrenshot(page);
            if (BrowserInitialized())
            {
                page.SetDownloadedData(this);
            }
            else
            {
                Logs.Log.WriteError("PuppeteerDownloader Download", "Unable to launch browser");
            }
        }

        public void SetContentAndScrenshot(Websites.Page page)
        {
            Content = null;
            Screenshot = null;

            try
            {
                var taskGetAsync = Task.Run(async () => await GetAsync(page));
                var taskDelay = Task.Delay(GetTimeout());                
                var completedTask = Task.WhenAny(taskGetAsync, taskDelay).Result;
                if (completedTask == taskGetAsync)
                {
                    taskGetAsync.Wait();
                    (Content, Screenshot) = taskGetAsync.Result;
                }
                CloseAllPagesExceptFirst();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader SetContentAndScrenshot", exception);
            }
        }

        private async Task<(string? content, byte[]? screenShot)> GetAsync(Websites.Page page)
        {
            string? content = null;
            byte[]? screenShot = null;

            try
            {
                if (BrowserInitialized())
                {
                    var browserPage = await GetBroserPage(Browser!, page.Website.LanguageCode, page.Uri);
                    await browserPage.GoToAsync(page.Uri.ToString(), NavigationOptions);
                    await browserPage.EvaluateExpressionAsync(ExpressionRemoveCookies);
                    if (Config.TAKE_SCREENSHOT)
                    {
                        screenShot = await PuppeteerScreenshot.TakeScreenshot(browserPage, page);
                    }
                    content = await browserPage.GetContentAsync();
                }
            }
            catch
            {

            }
            return (content, screenShot);
        }


        private async Task<IPage> GetBroserPage(IBrowser browser, LanguageCode languageCode, Uri uri)
        {
            IPage browserPage = await browser.NewPageAsync();
            if (Config.IsConfigurationProduction())
            {
                browserPage.DefaultNavigationTimeout = GetTimeout();
            }
            SetAccepLanguage(browserPage, languageCode);
            await browserPage.SetUserAgentAsync(Config.USER_AGENT);

            await browserPage.SetRequestInterceptionAsync(true);
            browserPage.Request += async (sender, e) => await HandleRequestAsync(e, uri);
            browserPage.Response += (sender, e) => HandleResponseAsync(e, uri);

            return browserPage;
        }

        private static void SetAccepLanguage(IPage browserPage, LanguageCode languageCode)
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

        private static async Task HandleRequestAsync(RequestEventArgs e, Uri uri)
        {
            try
            {
                if (BlockResources.Contains(e.Request.ResourceType) ||
                    BlockDomains.Contains(uri.Host) ||
                    e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0
                    //|| e.Request.IsNavigationRequest && e.Request.Url != uri.ToString()) // problematic
                    )
                {
                    await e.Request.AbortAsync();
                    return;
                }

                await e.Request.ContinueAsync();
            }
            catch //(Exception exception)
            {
                //Logs.Log.WriteLogErrors("PuppeteerDownloader HandleRequestAsync " + uri.ToString(), exception);
            }
        }

        private void HandleResponseAsync(ResponseCreatedEventArgs e, Uri uri)
        {
            try
            {
                if (!Uri.TryCreate(e.Response.Url, UriKind.RelativeOrAbsolute, out Uri? responseUri))
                {
                    return;
                }
                if (!responseUri.Equals(uri))
                {
                    return;
                }
                HttpStatusCode = (short)e.Response.Status;
                if (e.Response.Headers.TryGetValue("Location", out string? location))
                {
                    RedirectUrl = location;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader HandleResponseAsync", exception);
            }
        }

        private static int GetTimeout()
        {
            return Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
        }
    }
}
