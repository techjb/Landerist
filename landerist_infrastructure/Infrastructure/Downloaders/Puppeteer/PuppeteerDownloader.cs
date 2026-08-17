using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_library.Websites;
using PuppeteerSharp;
using System.Diagnostics;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer
{
    public class PuppeteerDownloader : IDownloader, IDownloaderSession
    {
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public short? HttpStatusCode { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;
        public string? Etag { get; set; } = null;
        public string? LastModified { get; set; } = null;

        private Pages.Page? Page;
        private readonly LaunchOptions launchOptions;
        private readonly PuppeteerBrowserOptions Options;
        private readonly IApplicationLogger Logger;

        private IBrowser? Browser;
        private IPage? BrowserPage;

        private const short ProxyAuthenticationRequiredStatusCode = 407;

        private readonly bool UseProxy = false;



        private bool BrowserChrashed = false;

        private readonly Credentials? ProxyCredentials;
        private bool FirstNavigationRequestReaded = false;
        private string CurrentExecutionStep = "Idle";

        public PuppeteerDownloader(
            bool useProxy,
            PuppeteerBrowserOptions options,
            IApplicationLogger logger,
            bool initializeSynchronously = true)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);
            Logger = logger;
            Options = options;
            UseProxy = useProxy;
            if (UseProxy)
            {
                ProxyCredentials = new Credentials
                {
                    Username = options.ProxyUsername,
                    Password = options.ProxyPassword
                };
            }

            launchOptions = PuppeteerLaunchOptionsFactory.Create(UseProxy, options);
            if (initializeSynchronously)
            {
                Browser = LaunchAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
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

        internal async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Browser = await LaunchAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<IBrowser?> LaunchAsync(CancellationToken cancellationToken)
            => await PuppeteerBrowserLifecycle.LaunchAsync(
                launchOptions,
                Logger,
                cancellationToken).ConfigureAwait(false);
        public void CloseBrowser()
        {
            CloseBrowserAsync().GetAwaiter().GetResult();
        }

        public async Task CloseBrowserAsync()
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

            await PuppeteerBrowserLifecycle.CloseBrowserAsync(browser, Logger)
                .ConfigureAwait(false);
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

            await PuppeteerBrowserLifecycle.ClosePageAsync(browserPage, Logger)
                .ConfigureAwait(false);
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

            //Logger.WriteInfo("PuppeteerTest", "Starting test");
            //string? text = new PuppeteerDownloader(true).GetText(Page);

            //Pages.Page page1 = new("https://www.rualcasa.com/ficha/local-comercial/alicante/babel/1008/21300773/es/");
            //var puppeteerDownloader = new PuppeteerDownloader(false);
            //Console.WriteLine(puppeteerDownloader.GetText(page1));

        }

        public void Download(Pages.Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            SetContentAndScrenshot(page);
            if (PageInitialized() && !BrowserHasChrashed())
            {
                page.SetDownloadedData(new PageDownloadResult(
                    Content,
                    Screenshot,
                    HttpStatusCode,
                    RedirectUrl,
                    Etag,
                    LastModified));
            }
        }

        public async Task DownloadAsync(
            Pages.Page page,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            await SetContentAndScreenshotAsync(page, cancellationToken)
                .ConfigureAwait(false);
            if (PageInitialized() && !BrowserHasChrashed())
            {
                page.SetDownloadedData(new PageDownloadResult(
                    Content,
                    Screenshot,
                    HttpStatusCode,
                    RedirectUrl,
                    Etag,
                    LastModified));
            }
        }
        public void SetContentAndScrenshot(Pages.Page page) =>
            SetContentAndScreenshotAsync(page, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

        public async Task SetContentAndScreenshotAsync(
            Pages.Page page,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            cancellationToken.ThrowIfCancellationRequested();

            Content = null;
            Screenshot = null;
            HttpStatusCode = null;
            RedirectUrl = null;
            Etag = null;
            LastModified = null;
            Page = page;

            int delay = GetTimeout(UseProxy);
            var stopwatch = Stopwatch.StartNew();
            SetExecutionStep("Starting download");

            try
            {
                Task<string?> download = GetAsync();
                PuppeteerDownloadExecutionResult<string?> result =
                    await PuppeteerDownloadExecution.WaitAsync(
                        download,
                        delay + 1000,
                        cancellationToken,
                        timedOutDownload => SetBrowserChrashed(BuildExecutionMessage(
                            "Timeout reached",
                            timedOutDownload,
                            delay,
                            stopwatch.ElapsedMilliseconds)),
                        ClosePageAsync).ConfigureAwait(false);
                if (!result.TimedOut)
                {
                    Content = result.Value;
                }
            }
            catch (Exception exception)
            {
                SetBrowserChrashed(BuildExecutionMessage(
                    "Exception occurred",
                    null,
                    delay,
                    stopwatch.ElapsedMilliseconds,
                    exception));
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
                var response = await PuppeteerNavigation.NavigateAsync(
                    BrowserPage!,
                    Page.Uri.ToString(),
                    Page.Website.NavigationWaitSelector,
                    GetTimeout(UseProxy),
                    SetExecutionStep,
                    ClosePageAsync).ConfigureAwait(false);
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
                    await BrowserPage!.EvaluateFunctionOnNewDocumentAsync("() => { delete navigator.__proto__.webdriver; }");
                }
                catch
                {

                }

                try
                {
                    SetExecutionStep("Removing web drivers");
                    await BrowserPage!.EvaluateExpressionAsync(PuppeteerPageScripts.DeleteWebdriver);
                }
                catch
                {

                }

                try
                {
                    SetExecutionStep("Removing invisible elements");
                    await BrowserPage!.EvaluateFunctionAsync(PuppeteerPageScripts.RemoveInvisibleElements);
                }
                catch //(Exception exception)
                {
                    //Logger.WriteInfo("PuppeteerDownloader ExpressionRemoveInvisibleElements", exception.Message);
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

                //Logger.WriteInfo("PuppeteerDownloader NavigationException", message);
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
                Logger.WriteInfo("PuppeterDownloader InitializePage", exception.Message);
                return;
            }

            if (BrowserPage is null)
            {
                return;
            }

            SetExecutionStep("Configuring browser page");
            await PuppeteerPageConfiguration.ConfigureAsync(
                BrowserPage,
                Page.Website,
                GetTimeout(UseProxy),
                (_, e) => _ = HandleRequestAsync(e),
                (_, e) => HandleResponseAsync(e)).ConfigureAwait(false);
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
                    case PuppeteerRequestAction.Abort:                        
                        await e.Request.AbortAsync();
                        return;
                }

                await e.Request.ContinueAsync();
            }
            catch //(Exception exception)
            {
                //Logger.WriteError("PuppeteerDownloader HandleRequestAsync " + uri.ToString(), exception);
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
                LastModified = GetHeaderValue(e.Response.Headers, "Last-Modified");
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
                Logger.WriteError("PuppeteerDownloader HandleResponseAsync", exception.ToString());
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

        private int GetTimeout(bool useProxy) =>
            Options.GetTimeoutMilliseconds(useProxy);

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
