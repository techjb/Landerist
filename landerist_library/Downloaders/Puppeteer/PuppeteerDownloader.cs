﻿using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Downloaders.Multiple;
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
            "--disable-features=TranslateUI",
            "--disable-features=ChromeLabs",
            "--disable-features=Translate",
            "--disable-features=LensStandalone",
            "--window-position=0,0",
            "--ignore-certificate-errors",
            "--ignore-certificate-errors-spki-list",
            "--disable-gpu",
            "--disable-dev-shm-usage",
            "--disable-background-timer-throttling",
            "--disable-renderer-backgrounding",
            "--incognito",
            "--disable-dev-profile",
            "--aggressive-cache-discard",
            "--disable-cache",
            "--disable-application-cache",
            "--disable-offline-load-stale-cache",
            "--disable-gpu-shader-disk-cache",
            "--media-cache-size=0",
            "--disk-cache-size=0",
            "--disable-gl-drawing-for-tests",
            "--disable-offline-load-stale-cache",
            "--disable-histograms",
            "--disk-cache-dir=null",
            "--no-experiments",
            "--no-default-browser-check",
            "--disable-background-timer-throttling",
            "--disable-backgrounding-occluded-windows",
            "--disable-notifications",
            "--disable-background-networking",
            "--disable-component-update",
          
            //"--single-process",
            //"--disable-web-security",
            //"--disable-extensions",
            //"--disable-plugins",            
            //"--disable-breakpad",
            //"--disable-client-side-phishing-detection",
            //"--disable-sync",
            //"--disable-translate",            
            //"--disable-default-apps",
            //"--mute-audio",            
            //"--disable-domain-reliability",
            //"--autoplay-policy=user-gesture-required",
            //"--disable-component-extensions-with-background-pages",


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
            //Headless = false,
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

        private static readonly HashSet<string> BlockedExtensions =
        [
            ".exe", ".zip", ".rar", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".tmp"
        ];

        private IBrowser? Browser;

        private IPage? BrowserPage;

        private static readonly NavigationOptions NavigationOptions = new()
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2],
            Timeout = GetTimeout(),
        };

        private bool BrowserChrashed = false;

        private readonly SingleDownloader? SingleDownloader;


        public PuppeteerDownloader(SingleDownloader singleDownloader) : this()
        {
            SingleDownloader = singleDownloader;
        }

        public PuppeteerDownloader()
        {
            Browser = Task.Run(LaunchAsync).Result;
        }

        public bool BrowserInitialized()
        {
            return Browser != null;
        }

        public bool PageInitialized()
        {
            return BrowserPage != null;
        }


        public bool BrowserHasChrashed()
        {
            return BrowserChrashed;
        }

        public bool BrowserPageInitialized()
        {
            return BrowserInitialized() && PageInitialized();
        }

        private async Task<IBrowser?> LaunchAsync()
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
            if (!BrowserInitialized())
            {
                return;
            }
            try
            {
                Browser!.CloseAsync();
                Browser!.Dispose();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader CloseBrowserAsync", exception);
            }
            Browser = null;
        }

        public void ClosePage()
        {
            if (!PageInitialized())
            {
                return;
            }
            try
            {
                Task.Run(async () => await BrowserPage!.CloseAsync()).Wait();
                BrowserPage!.Dispose();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader CloseBrowserAsync", exception);
            }
            BrowserPage = null;
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
            if (PageInitialized() && !BrowserHasChrashed())
            {
                page.SetDownloadedData(this);
            }
        }

        public void SetContentAndScrenshot(Websites.Page page)
        {
            Content = null;
            Screenshot = null;

            var delay = GetTimeout();

            try
            {
                var taskGetAsync = Task.Run(async () => await GetAsync(page));
                var taskDelay = Task.Delay(delay);
                var completedTask = Task.WhenAny(taskGetAsync, taskDelay).Result;
                if (completedTask == taskGetAsync)
                {
                    (Content, Screenshot) = taskGetAsync.Result;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader SetContentAndScrenshot Exception", exception);
            }
        }

        private async Task<(string? content, byte[]? screenShot)> GetAsync(Websites.Page page)
        {
            string? content = null;
            byte[]? screenShot = null;
            BrowserChrashed = false;

            try
            {
                if (!BrowserInitialized())
                {
                    BrowserChrashed = true;
                    return (content, screenShot);
                }
                await InitializePage(page.Website.LanguageCode, page.Uri);
                if (!PageInitialized())
                {
                    BrowserChrashed = true;
                    return (content, screenShot);
                }

                var response = await BrowserPage!.GoToAsync(page.Uri.ToString(), NavigationOptions);
                if (response.Ok)
                {
                    await BrowserPage.EvaluateExpressionAsync(ExpressionRemoveCookies);
                    if (Config.TAKE_SCREENSHOT)
                    {
                        screenShot = await PuppeteerScreenshot.TakeScreenshot(BrowserPage, page);
                    }
                    content = await BrowserPage.GetContentAsync();
                    return (content, screenShot);
                }
            }
            //catch (NullReferenceException exception)
            //{
            //    Logs.Log.WriteError("PuppeterDownloader GetAsync NullReferenceException", exception.Message);
            //}
            //catch (TargetClosedException exception)
            //{
            //    Logs.Log.WriteError("PuppeterDownloader GetAsync TargetClosedException", exception.Message);
            //}
            //catch (PuppeteerException exception)
            //{
            //    Logs.Log.WriteError("PuppeterDownloader GetAsync PuppeteerException", exception.Message);
            //}
            catch (NavigationException exception)
            {
                var message = SingleDownloader!.Id.ToString() + " " + exception.Message;
                //Logs.Log.WriteError("PuppeterDownloader GetAsync NavigationException", message);
            }
            catch (Exception exception)
            {
                BrowserChrashed = true;
                var message = SingleDownloader!.Id.ToString() + " " + exception.GetType().ToString();
                //Logs.Log.WriteError("PuppeterDownloader GetAsync Exception", message);
            }

            return (content, screenShot);
        }


        private async Task InitializePage(LanguageCode languageCode, Uri uri)
        {
            if (PageInitialized())
            {
                return;
            }

            try
            {
                var pages = Task.Run(async () => await Browser!.PagesAsync()).Result;
                if (pages.Length > 0)
                {
                    BrowserPage = pages[0];
                }
                else
                {
                    BrowserPage = await Browser!.NewPageAsync();
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteInfo("PageInitialized error", SingleDownloader!.Id.ToString() + " " + exception.Message);
                //Logs.Log.WriteError( SingleDownloader!.Id +  " PuppeteerDownloader InitializePage", exception);
                return;
            }


            BrowserPage.DefaultNavigationTimeout = GetTimeout();

            SetAccepLanguage(BrowserPage, languageCode);
            await BrowserPage.SetUserAgentAsync(Config.USER_AGENT);
            await BrowserPage.SetCacheEnabledAsync(false);

            await BrowserPage.SetRequestInterceptionAsync(true);
            BrowserPage.Request += async (sender, e) => await HandleRequestAsync(e, uri);
            BrowserPage.Response += (sender, e) => HandleResponseAsync(e, uri);
            //await browserPage.Client.SendAsync("HeapProfiler.collectGarbage");

        }

        private void SetAccepLanguage(IPage browserPage, LanguageCode languageCode)
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
            try
            {
                var url = e.Request.Url.ToLower();

                if (BlockResources.Contains(e.Request.ResourceType) ||
                    BlockDomains.Contains(uri.Host) ||
                    BlockedExtensions.Any(url.EndsWith) ||
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
