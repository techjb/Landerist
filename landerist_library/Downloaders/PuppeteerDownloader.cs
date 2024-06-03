using HtmlAgilityPack;
using PuppeteerSharp;
using landerist_library.Configuration;
using landerist_library.Websites;
using System.Diagnostics;


namespace landerist_library.Downloaders
{
    public class PuppeteerDownloader : IDownloader
    {
        public short? HttpStatusCode { get; set; } = null;
        public string? Content { get; set; } = null;
        public byte[]? Screenshot { get; set; } = null;
        public string? RedirectUrl { get; set; } = null;

        private bool BrowserLaunched = false;

        private static readonly ScreenshotType ScreenshotType = ScreenshotType.Png;

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


        private static readonly string[] LaunchOptionsArgs = Config.TAKE_SCREENSHOT ?
            [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-position=0,0",
                "--ignore-certificate-errors",
                "--ignore-certificate-errors-spki-list",
                "--disable-extensions-except=" + IDontCareAboutCookies,
                "--load-extension=" + IDontCareAboutCookies
            ] :
            [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-position=0,0",
                "--ignore-certificate-errors",
                "--ignore-certificate-errors-spki-list",
            ];

        private static readonly LaunchOptions launchOptions = new()
        {
            //Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);            
            Headless = Config.IsConfigurationProduction(),
            Devtools = false,
            IgnoreHTTPSErrors = true,
            Args = LaunchOptionsArgs,
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


        public static void UpdateChromeAndDoTest()
        {
            UpdateChrome();
            DoTest();
        }

        public static void UpdateChrome()
        {
            bool sucess = Task.Run(DownloadBrowserAsync).Result;
            Logs.Log.WriteLogInfo("service", "Updating Chrome. Success: " + sucess);
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
                    Logs.Log.WriteLogErrors("PuppeterDownloader", exception);
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


            Logs.Log.WriteLogInfo("PuppeterTest", "Starting test");
            string? text = new PuppeteerDownloader().GetText(page);

            Console.WriteLine(text);
            Logs.Log.WriteLogInfo("PuppeterTest", "Result: " + text);
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
                    Logs.Log.WriteLogErrors("PuppeteerDownloader GetText", exception);
                }
            }
            return null;
        }


        public void Download(Websites.Page page)
        {
            SetContentAndScrenshot(page);
            if (BrowserLaunched)
            {
                page.SetDownloadedData(this);
            }
            else
            {
                Logs.Log.WriteLogErrors("PuppeterDownloader Download", "Unable to launch browser");
            }
        }

        public void SetContentAndScrenshot(Websites.Page page)
        {
            try
            {
                (Content, Screenshot) = Task.Run(async () => await GetAsync(page)).Result;                
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("PuppeterDownloader GetContent", exception);
            }
        }

        private async Task<(string? content, byte[]? screenShot)> GetAsync(Websites.Page page)
        {
            string? content = null;
            byte[]? screenShot = null;

            IBrowser? browser = null;
            IPage? browserPage = null;

            try
            {
                browser = await Puppeteer.LaunchAsync(launchOptions);
                BrowserLaunched = true;

                browserPage = await GetBroserPage(browser, page.Website.LanguageCode, page.Uri);
                await browserPage.GoToAsync(page.Uri.ToString(), WaitUntilNavigation.Networkidle0);

                content = await browserPage.GetContentAsync();
                screenShot = await TakeScreenshot(browserPage);
            }
            catch
            {

            }
            finally
            {
                if (browserPage != null)
                {
                    await browserPage.CloseAsync();
                }
                if (browser != null)
                {
                    await browser.CloseAsync();
                    browser.Dispose();
                }
            }
            return (content, screenShot);
        }

        private static async Task<byte[]?> TakeScreenshot(IPage browserPage)
        {
            if (!Config.TAKE_SCREENSHOT)
            {
                return null;
            }

            ScreenshotOptions screenshotOptions = new()
            {
                Type = ScreenshotType,
                FullPage = true,
                OmitBackground = true,
            };
            if (ScreenshotType.Equals(ScreenshotType.Jpeg))
            {
                screenshotOptions.Quality = 90;
            }
            try
            {
                return await browserPage.ScreenshotDataAsync(screenshotOptions);                
            }
            catch(Exception exception) 
            {
                Logs.Log.WriteLogErrors("PuppeteerDownloader TakeScreenshot", exception);
            }            
            return null;
        }


        private async Task<IPage> GetBroserPage(IBrowser browser, LanguageCode languageCode, Uri uri)
        {
            IPage browserPage = await browser.NewPageAsync();
            if (Config.IsConfigurationProduction())
            {
                browserPage.DefaultNavigationTimeout = Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000;
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
            if (BlockResources.Contains(e.Request.ResourceType))
            {
                await AbortRequest(e);
                return;
            }
            if (BlockDomains.Contains(uri.Host))
            {
                await AbortRequest(e);
                return;
            }
            if (e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0)
            {
                await AbortRequest(e);
                return;
            }

            // problematic
            //if (e.Request.IsNavigationRequest && e.Request.Url != uri.ToString())
            //{
            //    //await e.Request.AbortAsync();
            //    //return;
            //}
            await e.Request.ContinueAsync();
        }

        private static async Task AbortRequest(RequestEventArgs e)
        {
            try
            {
                await e.Request.AbortAsync();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("PuppeterDownloader AbortRequest", exception);
            }
            return;
            
        }

        private void HandleResponseAsync(ResponseCreatedEventArgs e, Uri uri)
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
    }
}
