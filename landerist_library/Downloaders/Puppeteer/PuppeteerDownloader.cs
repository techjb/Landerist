using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Websites;
using PuppeteerSharp;
using System.Diagnostics;

namespace landerist_library.Downloaders.Puppeteer
{
    public class PuppeteerDownloader : IDownloader, IDownloaderSession
    {
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public short? HttpStatusCode { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;
        public string? Etag { get; set; } = null;

        private Pages.Page? Page;
        private readonly LaunchOptions launchOptions;

        private IBrowser? Browser;
        private IPage? BrowserPage;

        private const short ProxyAuthenticationRequiredStatusCode = 407;

        private readonly bool UseProxy = false;



        private bool BrowserChrashed = false;

        private readonly Credentials? ProxyCredentials;
        private bool FirstNavigationRequestReaded = false;
        private string CurrentExecutionStep = "Idle";

        public PuppeteerDownloader(bool useProxy)
        {
            UseProxy = useProxy;
            if (UseProxy)
            {
                ProxyCredentials = new Credentials
                {
                    Username = PrivateConfig.PROXY_USERNAME,
                    Password = PrivateConfig.PROXY_PASSWORD
                };
            }

            launchOptions = PuppeteerLaunchOptionsFactory.Create(UseProxy);
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

            //Pages.Page page1 = new("https://www.rualcasa.com/ficha/local-comercial/alicante/babel/1008/21300773/es/");
            //var puppeteerDownloader = new PuppeteerDownloader(false);
            //Console.WriteLine(puppeteerDownloader.GetText(page1));

        }

        private string? GetText(Pages.Page page)
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

        public void Download(Pages.Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            SetContentAndScrenshot(page);
            if (PageInitialized() && !BrowserHasChrashed())
            {
                page.SetDownloadedData(this);
            }
        }

        public void SetContentAndScrenshot(Pages.Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            Content = null;
            Screenshot = null;
            HttpStatusCode = null;
            RedirectUrl = null;
            Etag = null;
            Page = page;

            var delay = GetTimeout(UseProxy);
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
                    Content = taskGetAsync.GetAwaiter().GetResult();
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
            }
        }

        private async Task<string?> GetAsync()
        {
            string? content = null;
            BrowserChrashed = false;
            if (Browser is null)
            {
                return null;
            }

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
                    await BrowserPage!.EvaluateExpressionAsync(PuppeteerPageScripts.RemoveCookies);
                }
                catch //(Exception exception)
                {
                    //Logs.Log.WriteInfo("PuppeteerDownloader ExpressionRemoveCookies", exception.Message);
                }

                try
                {
                    SetExecutionStep("Removing invisible elements");
                    await BrowserPage!.EvaluateFunctionAsync(PuppeteerPageScripts.RemoveInvisibleElements);
                }
                catch //(Exception exception)
                {
                    //Logs.Log.WriteInfo("PuppeteerDownloader ExpressionRemoveInvisibleElements", exception.Message);
                }

                SetExecutionStep("Reading page content");
                content = await BrowserPage!.GetContentAsync();
                SetExecutionStep("Completed");
                return content;
            }

            catch (NavigationException exception)
            {
                var message =
                           $"{HttpStatusCode} " +
                           $"{UseProxy} " +
                           $"{exception.Message} " +
                           $"{Page?.Uri}";

                if (UseProxy && HttpStatusCode == ProxyAuthenticationRequiredStatusCode)
                {
                    SetBrowserChrashed("Proxy authentication failed: " + message);
                    return content;
                }

                //Logs.Log.WriteInfo("PuppeteerDownloader NavigationException", message);
            }
            catch (Exception exception)
            {
                SetBrowserChrashed("Exception occurred: " + exception.Message);
                var message =
                       $"HttpStatusCode: {HttpStatusCode} " +
                       $"UseProxy: {UseProxy} " +
                       $"Message: {exception.Message}";

                Console.WriteLine("Exception " + message);
            }

            return content;
        }

        private async Task<IResponse?> NavigateWithTimeoutAsync(string url)
        {
            var timeout = GetTimeout(UseProxy);

            NavigationOptions NavigationOptions = new()
            {
                WaitUntil = [WaitUntilNavigation.Networkidle2],
                Timeout = timeout
            };

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

            BrowserPage.DefaultNavigationTimeout = GetTimeout(UseProxy);
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

                var action = PuppeteerRequestRules.GetAction(e, currentPage);
                switch (action)
                {
                    case PuppeteerRequestAction.RespondWithTransparentGif:
                        await e.Request.RespondAsync(PuppeteerRequestRules.CreateTransparentGifResponse());
                        return;
                    case PuppeteerRequestAction.Abort:
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

        private void HandleResponseAsync(ResponseCreatedEventArgs e)
        {
            try
            {
                if (Page is null)
                {
                    return;
                }

                if (!e.Response.Request.IsNavigationRequest || FirstNavigationRequestReaded)
                {
                    return;
                }

                FirstNavigationRequestReaded = true;

                Uri.TryCreate(e.Response.Url, UriKind.Absolute, out Uri? responseUri);
                Uri? redirectUri = responseUri;

                HttpStatusCode = (short)e.Response.Status;
                Etag = GetHeaderValue(e.Response.Headers, "ETag");
                var location = GetHeaderValue(e.Response.Headers, "Location");
                if (!string.IsNullOrWhiteSpace(location))
                {
                    if (!Uri.TryCreate(location, UriKind.Absolute, out redirectUri))
                    {
                        Uri.TryCreate(responseUri ?? Page.Uri, location, out redirectUri);
                    }
                }

                if (redirectUri != null && redirectUri.IsAbsoluteUri && !Page.Uri.Equals(redirectUri))
                {
                    RedirectUrl = redirectUri.ToString();
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("PuppeteerDownloader HandleResponseAsync", exception);
            }
        }

        private static string? GetHeaderValue(Dictionary<string, string> headers, string headerName)
        {
            foreach (var header in headers)
            {
                if (header.Key.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                {
                    return string.IsNullOrWhiteSpace(header.Value) ? null : header.Value.Trim();
                }
            }

            return null;
        }

        private static int GetTimeout(bool useProxy)
        {
            var timeout = Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
            if (useProxy)
            {
                timeout *= 2;
            }
            return timeout;
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
