using PuppeteerSharp;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer
{
    internal static class PuppeteerLaunchOptionsFactory
    {
        private static readonly string[] DefaultArgs =
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
        ];

        public static LaunchOptions Create(bool useProxy, PuppeteerBrowserOptions options)
        {
            return new LaunchOptions
            {
                //Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);
                Headless = options.Headless,
                Devtools = false,
                //IgnoreHTTPSErrors = true,
                Args = BuildArgs(useProxy, options),
            };
        }

        private static string[] BuildArgs(bool useProxy, PuppeteerBrowserOptions options)
        {
            return useProxy ? [.. DefaultArgs, BuildProxyServerArgument(options)] : DefaultArgs;
        }

        private static string BuildProxyServerArgument(PuppeteerBrowserOptions options)
        {
            return "--proxy-server=" + options.ProxyHost + ":" + options.GetProxyPort();
        }

    }
}
