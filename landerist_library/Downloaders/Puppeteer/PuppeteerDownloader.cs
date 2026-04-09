using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Downloaders.Multiple;
using landerist_library.Websites;
using PuppeteerSharp;
using System.Diagnostics;

namespace landerist_library.Downloaders.Puppeteer
{
    public class PuppeteerDownloader : IDownloader
    {
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public short? HttpStatusCode { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;

        private Websites.Page? Page;

        private const string ExpressionRemoveCookies =
            @"document.querySelectorAll('[class*=""cookie"" i], [id*=""cookie"" i]').forEach(el => el.remove());";

        private const string ExpressionRemoveInvisibleElements = @"
            () => {
                const isVisible = (elem) => {
                    if (!(elem instanceof Element)) return false;
                    const style = window.getComputedStyle(elem);
                    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
                        return false;
                    }
                    return true;
                };

                const removeInvisibleElements = (root) => {
                    if (!(root instanceof Node)) {
                        return;
                    }

                    const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT, null, false);
                    const toRemove = [];
                    while (walker.nextNode()) {
                        const node = walker.currentNode;
                        if (!isVisible(node)) {
                            toRemove.push(node);
                        }
                    }
                    for (const node of toRemove) {
                        node.remove();
                    }
                };

                const root = document.body ?? document.documentElement;
                removeInvisibleElements(root);
            }
        ";

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
            //"--disable-component-extensions-with-background-Pages",
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
            Headless = Config.HEADLESS_BROWSER,
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
            //WaitUntil = [WaitUntilNavigation.Networkidle2],
            WaitUntil = [WaitUntilNavigation.Networkidle2],
            Timeout = GetTimeout()
        };

        private bool BrowserChrashed = false;
        private readonly SingleDownloader? SingleDownloader;
        private readonly bool UseProxy = false;
        private readonly Credentials? ProxyCredentials;
        private bool FirstNavigationRequestReaded = false;
        private string CurrentExecutionStep = "Idle";

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

            Browser = LaunchAsync().GetAwaiter().GetResult();
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
            CloseBrowserAsync().GetAwaiter().GetResult();
        }

        private async Task CloseBrowserAsync()
        {
            if (!BrowserInitialized())
            {
                return;
            }

            var browser = Browser;
            Browser = null;
            BrowserPage = null;

            if (browser is null)
            {
                return;
            }

            try
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader CloseBrowserAsync", exception);
            }
        }

        public void ClosePage()
        {
            ClosePageAsync().GetAwaiter().GetResult();
        }

        private async Task ClosePageAsync()
        {
            if (!PageInitialized())
            {
                return;
            }

            var browserPage = BrowserPage;
            BrowserPage = null;

            if (browserPage is null)
            {
                return;
            }

            try
            {
                await browserPage.CloseAsync();
                await browserPage.DisposeAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader ClosePageAsync", exception);
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
            var puppeteerDownloader = new PuppeteerDownloader(false);
            //Console.WriteLine(puppeteerDownloader.GetText(page1));

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
            ArgumentNullException.ThrowIfNull(page);

            SetContentAndScrenshot(page);
            if (PageInitialized() && !BrowserHasChrashed())
            {
                page.SetDownloadedData(this);
            }
        }

        public void SetContentAndScrenshot(Websites.Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            Content = null;
            Screenshot = null;
            HttpStatusCode = null;
            RedirectUrl = null;
            Page = page;

            var delay = GetTimeout();
            if (Config.IsConfigurationLocal())
            {
                delay = 1000 * 1000;
            }

            var stopwatch = Stopwatch.StartNew();
            SetExecutionStep("Starting download");

            try
            {
                var taskGetAsync = GetAsync();
                var taskDelay = Task.Delay(delay + 1000);
                var completedTask = Task.WhenAny(taskGetAsync, taskDelay).GetAwaiter().GetResult();

                if (completedTask == taskGetAsync)
                {
                    (Content, Screenshot) = taskGetAsync.GetAwaiter().GetResult();
                }
                else
                {
                    SetBrowserChrashed(BuildExecutionMessage("Timeout reached", taskGetAsync, delay, stopwatch.ElapsedMilliseconds));
                    ClosePage();
                }
            }
            catch (Exception exception)
            {
                SetBrowserChrashed(BuildExecutionMessage("Exception occurred", null, delay, stopwatch.ElapsedMilliseconds, exception));
                //Logs.Log.WriteError("PuppeteerDownloader SetContentAndScrenshot Exception", exception);
            }
        }

        private async Task<(string? content, byte[]? screenShot)> GetAsync()
        {
            string? content = null;
            byte[]? screenShot = null;
            BrowserChrashed = false;

            try
            {
                SetExecutionStep("Validating browser state");
                if (!BrowserInitialized())
                {
                    throw new Exception("Browser is not initialized.");
                }

                if (Page is null)
                {
                    throw new Exception("Page is not initialized.");
                }

                SetExecutionStep("Initializing page");
                await InitializePage();
                if (!PageInitialized())
                {
                    throw new Exception("Browser page is not initialized.");
                }

                if (UseProxy && ProxyCredentials is not null)
                {
                    SetExecutionStep("Authenticating proxy");
                    await BrowserPage!.AuthenticateAsync(ProxyCredentials);
                }

                SetExecutionStep("Navigating");
                var response = await NavigateWithTimeoutAsync(Page.Uri.ToString());
                if (response == null)
                {
                    throw new NavigationException("Response is null.");
                }

                if (!response.Ok)
                {
                    throw new NavigationException("Response is not Ok.");
                }

                try
                {
                    SetExecutionStep("Removing cookie banners");
                    await BrowserPage.EvaluateExpressionAsync(ExpressionRemoveCookies);
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteInfo("PuppeteerDownloader ExpressionRemoveCookies", exception.Message);
                }

                try
                {
                    SetExecutionStep("Removing invisible elements");
                    await BrowserPage.EvaluateFunctionAsync(ExpressionRemoveInvisibleElements);
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteInfo("PuppeteerDownloader ExpressionRemoveInvisibleElements", exception.Message);
                }

                if (Config.TAKE_SCREENSHOT)
                {
                    SetExecutionStep("Taking screenshot");
                    screenShot = await PuppeteerScreenshot.TakeScreenshot(BrowserPage, Page);
                }

                SetExecutionStep("Reading page content");
                content = await BrowserPage.GetContentAsync();
                SetExecutionStep("Completed");
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
                           $"{Page?.Uri}";

                //Console.WriteLine("NavigationException " + message);
                //Logs.Log.WriteError("PuppeterDownloader GetAsync NavigationException", message);    


            }
            catch (Exception exception)
            {
                SetBrowserChrashed("Exception occurred: " + exception.Message);
                var message =
                       $"HttpStatusCode: {HttpStatusCode} " +
                       $"UseProxy: {UseProxy} " +
                       //$"SingleDownloader Id:{SingleDownloader!.Id} " +
                       //$"ScrapedCounter:{SingleDownloader!.ScrapedCounter()} " +
                       $"Message: {exception.Message}";

                Console.WriteLine("Exception " + message);
                //Logs.Log.WriteError("PuppeterDownloader GetAsync Exception", message);
            }

            return (content, screenShot);
        }

        private async Task<IResponse?> NavigateWithTimeoutAsync(string url)
        {
            var timeout = GetTimeout();
            var navigationTask = BrowserPage!.GoToAsync(url, NavigationOptions);
            var completedTask = await Task.WhenAny(navigationTask, Task.Delay(timeout));

            if (completedTask == navigationTask)
            {
                return await navigationTask;
            }

            SetExecutionStep("Navigation timeout");
            await ClosePageAsync();
            throw new NavigationException($"Navigation timed out after {timeout} ms.");
        }

        private async Task InitializePage()
        {
            FirstNavigationRequestReaded = false;
            if (PageInitialized())
            {
                return;
            }

            if (Browser is null || Page is null)
            {
                return;
            }

            try
            {
                SetExecutionStep("Getting browser pages");
                var pages = await Browser.PagesAsync();
                if (pages.Length > 0)
                {
                    SetExecutionStep("Reusing browser page");
                    BrowserPage = pages[0];
                }
                else
                {
                    SetExecutionStep("Creating browser page");
                    BrowserPage = await Browser.NewPageAsync();
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteInfo("PuppeterDownloader InitializePage", exception.Message);
                return;
            }

            if (BrowserPage is null)
            {
                return;
            }

            BrowserPage.DefaultNavigationTimeout = GetTimeout();
            SetExecutionStep("Configuring browser page");
            await SetAcceptLanguageAsync(BrowserPage, Page.Website.LanguageCode);
            await BrowserPage.SetUserAgentAsync(Config.USER_AGENT);
            await BrowserPage.SetCacheEnabledAsync(false);
            await BrowserPage.SetRequestInterceptionAsync(true);
            BrowserPage.Request += (_, e) => _ = HandleRequestAsync(e);
            BrowserPage.Response += (_, e) => HandleResponseAsync(e);
        }

        private static async Task SetAcceptLanguageAsync(IPage browserPage, LanguageCode languageCode)
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
                await browserPage.SetExtraHttpHeadersAsync(extraHeaders);
            }
        }

        private async Task HandleRequestAsync(RequestEventArgs e)
        {
            try
            {
                var currentPage = Page;
                if (currentPage is null)
                {
                    await e.Request.ContinueAsync();
                    return;
                }

                var requestHost = currentPage.Uri.Host;
                if (Uri.TryCreate(e.Request.Url, UriKind.Absolute, out Uri? requestUri))
                {
                    requestHost = requestUri.Host;
                }

                var url = e.Request.Url.ToLowerInvariant();
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

        private void HandleResponseAsync(ResponseCreatedEventArgs e)
        {
            try
            {
                var currentPage = Page;
                if (currentPage is null)
                {
                    return;
                }

                if (!e.Response.Request.IsNavigationRequest || FirstNavigationRequestReaded)
                {
                    return;
                }

                FirstNavigationRequestReaded = true;

                Uri? redirectUri = null;
                Uri.TryCreate(e.Response.Url, UriKind.Absolute, out redirectUri);

                HttpStatusCode = (short)e.Response.Status;
                if (e.Response.Headers.TryGetValue("Location", out string? location))
                {
                    if (!Uri.TryCreate(location, UriKind.Absolute, out redirectUri))
                    {
                        Uri.TryCreate(currentPage.Uri, location, out redirectUri);
                    }
                }

                if (redirectUri != null && redirectUri.IsAbsoluteUri && !currentPage.Uri.Equals(redirectUri))
                {
                    RedirectUrl = redirectUri.ToString();
                    //new Database.RedirectUrl().Insert(Page!.Uri.ToString(), RedirectUrl);
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

        private void SetExecutionStep(string step)
        {
            CurrentExecutionStep = step;
        }

        private string BuildExecutionMessage(string prefix, Task? task = null, int? timeout = null, long? elapsedMilliseconds = null, Exception? exception = null)
        {
            var exceptionMessage = exception is null
                ? string.Empty
                : $" | Exception: {exception.GetType().Name}: {exception.Message}";

            return
                $"{prefix}. " +
                // $"Url: {Page?.Uri} | " +
                $"Step: {CurrentExecutionStep} | " +
                $"TaskStatus: {task?.Status} | " +
                // $"TimeoutMs: {timeout} | " +
                //$"ElapsedMs: {elapsedMilliseconds} | " +
                //$"BrowserInitialized: {BrowserInitialized()} | " +
                //$"PageInitialized: {PageInitialized()} | " +
                //$"HttpStatusCode: {HttpStatusCode} | " +
                //$"RedirectUrl: {RedirectUrl} | " +
                //$"UseProxy: {UseProxy}" +
                exceptionMessage;
        }

        private void SetBrowserChrashed(string message)
        {
            Console.WriteLine(message);
            BrowserChrashed = true;
        }
    }
}
