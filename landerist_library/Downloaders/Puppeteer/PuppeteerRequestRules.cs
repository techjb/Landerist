using landerist_library.Configuration;
using PuppeteerSharp;

namespace landerist_library.Downloaders.Puppeteer
{
    internal static class PuppeteerRequestRules
    {
        private static readonly HashSet<ResourceType> BlockedResourceTypes = Config.TAKE_SCREENSHOT ?
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

        private static readonly HashSet<ResourceType> ImageResourceTypes =
        [
            ResourceType.Image,
            ResourceType.ImageSet,
            ResourceType.Img,
        ];

        private static readonly HashSet<ResourceType> StylesAndScriptResourceTypes =
        [
            ResourceType.StyleSheet,
            ResourceType.Script,
        ];

        private static readonly HashSet<ResourceType> StyleResourceTypes =
        [
            ResourceType.StyleSheet,
        ];

        private static readonly HashSet<string> BlockedDomains = new(StringComparer.OrdinalIgnoreCase)
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
            "cercalia.com",
            "content-autofill.googleapis.com",
            "android.clients.google.com",
            "accounts.google.com"
        };

        private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".zip", ".rar", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".tmp"
        };

        private static readonly byte[] TransparentGifBytes =
            Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==");

        public static ResponseData CreateTransparentGifResponse()
        {
            return new ResponseData
            {
                Status = System.Net.HttpStatusCode.OK,
                ContentType = "image/gif",
                BodyData = TransparentGifBytes
            };
        }

        public static PuppeteerRequestAction GetAction(RequestEventArgs e, Pages.Page currentPage)
        {
            if (BlockedResourceTypes.Contains(e.Request.ResourceType))
            {
                return ImageResourceTypes.Contains(e.Request.ResourceType)
                    ? PuppeteerRequestAction.RespondWithTransparentGif
                    : PuppeteerRequestAction.Abort;
            }

            if (currentPage.Website.IsServihabitat() &&
                StylesAndScriptResourceTypes.Contains(e.Request.ResourceType))
            {
                return PuppeteerRequestAction.Abort;
            }

            if (currentPage.Website.IsEngelsAnVolgers() &&
                StyleResourceTypes.Contains(e.Request.ResourceType))
            {
                return PuppeteerRequestAction.Abort;
            }

            var requestHost = GetRequestHost(e.Request.Url, currentPage.Uri.Host);
            var url = e.Request.Url.ToLowerInvariant();
            if (BlockedDomains.Contains(requestHost) ||
                BlockedExtensions.Any(url.EndsWith) ||
                e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0)
            {
                return PuppeteerRequestAction.Abort;
            }

            return PuppeteerRequestAction.Continue;
        }

        private static string GetRequestHost(string url, string defaultHost)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? requestUri)
                ? requestUri.Host
                : defaultHost;
        }
    }

    internal enum PuppeteerRequestAction
    {
        Continue,
        Abort,
        RespondWithTransparentGif,
    }
}
