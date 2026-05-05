using landerist_library.Configuration;
using PuppeteerSharp;

namespace landerist_library.Downloaders.Puppeteer
{
    internal static class PuppeteerLaunchOptionsFactory
    {
        private static readonly string IDontCareAboutCookies = Config.CHROME_EXTENSIONS_DIRECTORY
            + "IDontCareAboutCookies\\1.0.1_0\\";

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

        private static readonly string[] ScreenshotArgs =
        [
            "--disable-extensions-except=" + IDontCareAboutCookies,
            "--load-extension=" + IDontCareAboutCookies
        ];

        public static LaunchOptions Create(bool useProxy)
        {
            return new LaunchOptions
            {
                //Headless = true, // if false, maybe need to comment await browserPage.SetRequestInterceptionAsync(true);
                Headless = Config.HEADLESS_BROWSER,
                Devtools = false,
                //IgnoreHTTPSErrors = true,
                Args = BuildArgs(useProxy),
            };
        }

        private static string[] BuildArgs(bool useProxy)
        {
            var args = Config.TAKE_SCREENSHOT ? [.. DefaultArgs, .. ScreenshotArgs] : DefaultArgs;

            return useProxy ? [.. args, BuildProxyServerArgument()] : args;
        }

        private static string BuildProxyServerArgument()
        {
            return "--proxy-server=" + PrivateConfig.PROXY_HOST + ":" + GetProxyPort();
        }

        private static string GetProxyPort()
        {
            if (!PrivateConfig.PROXY_RANDOMIZE_STICKY_PORTS ||
                PrivateConfig.PROXY_STICKY_PORT_MIN > PrivateConfig.PROXY_STICKY_PORT_MAX)
            {
                return PrivateConfig.PROXY_PORT;
            }

            return Random.Shared
                .Next(PrivateConfig.PROXY_STICKY_PORT_MIN, PrivateConfig.PROXY_STICKY_PORT_MAX + 1)
                .ToString();
        }
    }
}
