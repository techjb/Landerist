﻿using HtmlAgilityPack;
using PuppeteerSharp;
using landerist_library.Configuration;
using landerist_library.Websites;
using System.Diagnostics;


namespace landerist_library.Downloaders
{
    public class PuppeteerDownloader : IDownloader
    {
        private short? HttpStatusCode = null;

        private string? RedirectUrl = null;

        private string? Html = null;

        private bool PuppeterLaunched = false;

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
            Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);            
            //Headless = false,
            Devtools = false,
            IgnoreHTTPSErrors = true,
            Args = [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-position=0,0",
                "--ignore-certificate-errors",
                "--ignore-certificate-errors-spki-list",
            ],
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

        public static void DoTest()
        {
            // working
            var page = new Websites.Page("https://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");

            // http - > https
            //var page = new Websites.Page("http://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");
            //var page = new Websites.Page("http://www.inmogyb.com/es/buscador/alquiler/es/buscador/alquiler/trastero");

            // redirect example:
            //var page = new Websites.Page("https://www.realestate.bnpparibas.es/es/soluciones-medida/soluciones-para-inversores");


            Logs.Log.WriteLogInfo("PuppeterTest", "Starting test");
            var text = new PuppeteerDownloader().GetText(page);

            Console.WriteLine(text);
            Logs.Log.WriteLogInfo("PuppeterTest", "Result: " + text);
        }

        public string? GetText(Websites.Page page)
        {
            //DownloadBrowserAsync().Result;
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
        
        public static void InstallChrome()
        {
            Console.WriteLine("Installing Chrome ..");
            Logs.Log.WriteLogInfo("service", "Installing Chrome ..");

            bool sucess = Task.Run(async () => await DownloadBrowserAsync()).Result;

            Console.WriteLine("Success: " + sucess);
            Logs.Log.WriteLogInfo("service", "Success: " + sucess);
        }

        private static async Task<bool> DownloadBrowserAsync()
        {
            try
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetResponseBodyAndStatusCode(Websites.Page page)
        {
            var responseBody = GetResponseBody(page);
            if (PuppeterLaunched)
            {
                page.SetResponseBodyAndStatusCode(responseBody, HttpStatusCode);
            }
        }

        public string? GetResponseBody(Websites.Page page)
        {
            try
            {
                Html = Task.Run(async () => await GetAsync(page.Website.LanguageCode, page.Uri)).Result;
                PuppeterLaunched = true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("PuppeterDownloader GetResponseBody", exception);
            }
            return Html;
        }

        private async Task<string?> GetAsync(LanguageCode languageCode, Uri uri)
        {
            IBrowser? browser = null;
            IPage? browserPage = null;

            try
            {
                browser = await Puppeteer.LaunchAsync(launchOptions);
                browserPage = await GetBroserPage(browser, languageCode, uri);
                await browserPage.GoToAsync(uri.ToString(), WaitUntilNavigation.Networkidle0);
                return await browserPage.GetContentAsync();
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
            return null;
        }

        private async Task<IPage> GetBroserPage(IBrowser browser, LanguageCode languageCode, Uri uri)
        {
            var browserPage = await browser.NewPageAsync();
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

            // problematic
            //if (e.Request.IsNavigationRequest && e.Request.Url != uri.ToString())
            //{
            //    //await e.Request.AbortAsync();
            //    //return;
            //}
            await e.Request.ContinueAsync();
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

        public string? GetRedirectUrl()
        {
            return RedirectUrl;
        }

        public static void KillChrome()
        {
            Process[] processes = Process.GetProcessesByName("chrome");
            foreach (var process in processes)
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
    }
}
