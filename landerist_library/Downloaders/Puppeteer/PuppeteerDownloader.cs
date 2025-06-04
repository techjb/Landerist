using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Downloaders.Multiple;
using landerist_library.Websites;
using PuppeteerSharp;
using System;
using System.Diagnostics;
using System.Net;


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
            //"--incognito",
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
            "--disable-blink-features=AutomationControlled"
          
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

        private static readonly string[] LaunchOptionsProxy =
        [
            "--proxy-server=" + PrivateConfig.BRIGTHDATA_HOST + ":" + PrivateConfig.BRIGTHDATA_PORT + ""
        ];

        private static readonly string[] LaunchOptionsScreenShot =
        [
            "--disable-extensions-except=" + IDontCareAboutCookies,
            "--load-extension=" + IDontCareAboutCookies
        ];

        private readonly LaunchOptions launchOptions = new()
        {
            //Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);          
            Headless = Config.IsConfigurationProduction(),
            //Headless = false,
            Devtools = false,
            //IgnoreHTTPSErrors = true,            
            Args = Config.TAKE_SCREENSHOT ? [.. LaunchOptionsArgs, .. LaunchOptionsScreenShot] : LaunchOptionsArgs,
        };

        private static readonly HashSet<string> BlockDomains = new(StringComparer.OrdinalIgnoreCase)
        {
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
            "pippio.com",
            "static.addtoany.com",
        };

        private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".zip", ".rar", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".tmp"
        };

        private IBrowser? Browser;

        private IPage? BrowserPage;

        private static readonly NavigationOptions NavigationOptions = new()
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2],
            Timeout = GetTimeout()
        };

        private bool BrowserChrashed = false;

        private readonly SingleDownloader? SingleDownloader;

        private readonly bool UseProxy = false;

        private readonly Credentials? ProxyCredentials;

        private bool FirstNavigationRequestReaded = false;


        public PuppeteerDownloader(SingleDownloader singleDownloader) : this(singleDownloader.GetUseProxy())
        {
            SingleDownloader = singleDownloader;
        }

        public PuppeteerDownloader(bool useProxy)
        {
            UseProxy = useProxy;
            if (UseProxy)
            {
                var sessionId = Random.Shared.Next(1, 1_000_000);
                ProxyCredentials = new Credentials
                {
                    Username = $"{PrivateConfig.BRIGTHDATA_USERNAME}-session-{sessionId}",
                    //Username = $"{PrivateConfig.BRIGTHDATA_USERNAME}",
                    Password = PrivateConfig.BRIGTHDATA_PASSWORD
                };
                launchOptions.Args = [.. LaunchOptionsArgs, .. LaunchOptionsProxy];
            }
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

        public async void CloseBrowser()
        {
            if (!BrowserInitialized())
            {
                return;
            }
            try
            {
                await Browser!.CloseAsync();
                await Browser!.DisposeAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader CloseBrowserAsync", exception);
            }
            Browser = null;
        }

        public async void ClosePage()
        {
            if (!PageInitialized())
            {
                return;
            }
            try
            {
                await BrowserPage!.CloseAsync();
                await BrowserPage!.DisposeAsync();
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
            //Websites.Page Page = new("https://www.nbinmobiliaria.es/ad/99269515");
            //Websites.Page Page = new("http://www.finquesparellades.com/buscador/?Pagina=6");            

            // http - > https
            //var Page = new Websites.Page("http://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");
            //var Page = new Websites.Page("http://www.inmogyb.com/es/buscador/alquiler/es/buscador/alquiler/trastero");

            // redirect example: has to throw error
            //var Page = new Websites.Page("https://www.realestate.bnpparibas.es/es/soluciones-medida/soluciones-para-inversores");


            //Logs.Log.WriteInfo("PuppeteerTest", "Starting test");
            //string? text = new PuppeteerDownloader(true).GetText(Page);

            Websites.Page page1 = new("https://www.rualcasa.com/ficha/local-comercial/alicante/babel/1008/21300773/es/");
            Websites.Page page2 = new("https://www.ilanrealty.com/es/barcelona/barcelona/alquilar-propiedad-verano-vacaciones-con-familia-amigos-como-en-casa-todo-incluido-bcn30699.html");
            var puppeteerDownloader = new PuppeteerDownloader(true);
            Console.WriteLine(puppeteerDownloader.GetText(page1));
            Console.WriteLine(puppeteerDownloader.GetText(page2));

        }

        private string? GetText(Websites.Page page)
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
            HttpStatusCode = null;
            RedirectUrl = null;

            var delay = GetTimeout();
            if (Config.IsConfigurationLocal())
            {
                delay = 1000 * 1000;
            }

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
                    throw new Exception("Browser is not initialized.");
                }
                await InitializePage(page.Website.LanguageCode, page.Uri);
                if (!PageInitialized())
                {
                    throw new Exception("Page is not initialized.");
                }
                if (UseProxy)
                {
                    await BrowserPage!.AuthenticateAsync(ProxyCredentials);
                }
                var response = await BrowserPage!.GoToAsync(page.Uri.ToString(), NavigationOptions);
                if (response == null)
                {
                    throw new NavigationException("Response is null.");
                }
                if (!response.Ok)
                {
                    throw new NavigationException("Response is not Ok.");
                }
                await BrowserPage.EvaluateExpressionAsync(ExpressionRemoveCookies);
                if (Config.TAKE_SCREENSHOT)
                {
                    screenShot = await PuppeteerScreenshot.TakeScreenshot(BrowserPage, page);
                }
                content = await BrowserPage.GetContentAsync();
                return (content, screenShot);
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
                var message =
                       $"{HttpStatusCode} " +
                       $"{UseProxy} " +
                       //$"SingleDownloader Id:{SingleDownloader!.Id} " +
                       //$"ScrapedCounter:{SingleDownloader!.ScrapedCounter()} " +
                       $"{exception.Message} " +
                       $"{page.Uri}";
                       ;
                //Console.WriteLine("NavigationException " + message);
                //Logs.Log.WriteError("PuppeterDownloader GetAsync NavigationException", message);
            }
            catch (Exception exception)
            {
                BrowserChrashed = true;
                var message =
                       $"HttpStatusCode: {HttpStatusCode} " +
                       $"UseProxy: {UseProxy} " +
                       //$"SingleDownloader Id:{SingleDownloader!.Id} " +
                       //$"ScrapedCounter:{SingleDownloader!.ScrapedCounter()} " +
                       $"Message: {exception.Message}";

                //Console.WriteLine("Exception " + message);
                //Logs.Log.WriteError("PuppeterDownloader GetAsync Exception", message);
            }

            return (content, screenShot);
        }


        private async Task InitializePage(LanguageCode languageCode, Uri uri)
        {
            FirstNavigationRequestReaded = false;
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
                Logs.Log.WriteInfo("PuppeterDownloader InitializePage", exception.Message);
                return;
            }

            BrowserPage.DefaultNavigationTimeout = GetTimeout();
            SetAccepLanguage(BrowserPage, languageCode);
            await BrowserPage.SetUserAgentAsync(Config.USER_AGENT);
            await BrowserPage.SetCacheEnabledAsync(false);
            await BrowserPage.SetRequestInterceptionAsync(true);
            BrowserPage.Request += async (sender, e) => await HandleRequestAsync(e, uri);
            BrowserPage.Response += (sender, e) => HandleResponseAsync(e, uri);
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
                var requestHost = uri.Host;
                if(Uri.TryCreate(e.Request.Url,UriKind.Absolute, out Uri? requestUri))
                {
                    requestHost = requestUri.Host;
                }              

                if (BlockResources.Contains(e.Request.ResourceType) ||
                    BlockDomains.Contains(requestHost) ||
                    BlockedExtensions.Any(url.EndsWith) ||
                    e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0
                    //|| response.Request.IsNavigationRequest && response.Request.Url != uri.ToString()) // problematic
                    )
                {
                    await e.Request.AbortAsync();
                    return;
                }                
                //Console.WriteLine(e.Request.Url);   
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
                if (!e.Response.Request.IsNavigationRequest || FirstNavigationRequestReaded)
                {
                    return;
                }
                FirstNavigationRequestReaded = true;
                if (Uri.TryCreate(e.Response.Url, UriKind.RelativeOrAbsolute, out Uri? responseUri))
                {
                    if (!responseUri.Equals(uri))
                    {
                        RedirectUrl = responseUri.ToString();
                    }
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

        private static void LogHttpError(IResponse response)
        {
            //string error = $"▶ Status: {response.Status} ({response.StatusText}) ▶ Headers: ";
            //foreach (var kv in response.Headers)
            //{
            //    error+= $" {kv.Key}: {kv.Value} ";
            //}
            //error += $" ▶ URL: {response.Url}";
            string error = $"▶ Status: {response.Status} ({response.StatusText}) ▶ URL: {response.Url}";

            Logs.Log.WriteError("PuppeteerDownloader LogHttpError", error);
        }

        private static int GetTimeout()
        {
            return Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
        }
    }
}
