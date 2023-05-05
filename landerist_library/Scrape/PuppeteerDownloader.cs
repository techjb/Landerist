using PuppeteerSharp;
using System;

namespace landerist_library.Scrape
{
    public class PuppeteerDownloader
    {
        private static readonly HashSet<ResourceType> BlockResources = new()
        {
            ResourceType.Image,
            ResourceType.StyleSheet,
            ResourceType.Font,
            ResourceType.Media,
            //ResourceType.Other
        };

        private static readonly HashSet<string> BlockDomains = new()
        {
            "www.google-analytics.com"
        };


        public static string Get(Uri uri)
        {
            string text = Task.Run(async () => await GetAsync(uri)).Result;
            return text;
        }

        private static async Task<string> GetAsync(Uri uri)
        {
            // Descarga el navegador Chromium si es necesario
            //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            // Iniciar el navegador y la página
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                //ExecutablePath = "",
                Devtools = true,
            });


            using var page = await browser.NewPageAsync();
            // Evitar cargar imágenes y hojas de estilo
            await page.SetRequestInterceptionAsync(true);
            page.Request += async (sender, e) =>
            {
                if (BlockResources.Contains(e.Request.ResourceType))
                {
                    await e.Request.AbortAsync();
                    return;
                }
                Uri uri = new(e.Request.Url);
                if (BlockDomains.Contains(uri.Host))
                {
                    await e.Request.AbortAsync();
                    return;
                }
                await e.Request.ContinueAsync();
            };

            // Navega a la página web
            await page.GoToAsync(uri.ToString());

            // Extrae el texto de la página web
            //var pageText = await page.EvaluateFunctionAsync<string>("() => document.body.innerText");
            var pageText = await page.GetContentAsync();

            // Imprime el texto de la página web
            Console.WriteLine(pageText);

            return pageText;
        }
    }
}
