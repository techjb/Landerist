using System.Collections.Concurrent;
using PuppeteerSharp;

namespace landerist_library.Downloaders.Puppeteer
{
    internal static class PuppeteerRequestRules
    {
        private static readonly HashSet<ResourceType> BlockedResourceTypes = 
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

        private static readonly char[] ResourceTypeSeparators = [',', ';', '|', ' ', '\r', '\n', '\t'];

        private static readonly ConcurrentDictionary<string, AllowedResourceTypesCacheItem> AllowedResourceTypesCache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, BlockedDomainsCacheItem> BlockedDomainsCache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> DefaultBlockedDomains = new(StringComparer.OrdinalIgnoreCase)
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
            "accounts.google.com",
            "android.clients.google.com"
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
            if (IsFaviconRequest(e.Request.Url))
            {
                return PuppeteerRequestAction.Abort;
            }

            var allowedResourceTypes = GetAllowedResourceTypes(currentPage.Website.AllowedResourceTypes);
            if (allowedResourceTypes is not null)
            {
                if (!allowedResourceTypes.Contains(e.Request.ResourceType))
                {
                    return GetBlockedResourceAction(e.Request.ResourceType);
                }
            }
            else if (BlockedResourceTypes.Contains(e.Request.ResourceType))
            {
                return GetBlockedResourceAction(e.Request.ResourceType);
            }

            var requestHost = GetRequestHost(e.Request.Url, currentPage.Uri.Host);
            var url = e.Request.Url.ToLowerInvariant();            
            if (GetBlockedDomains(currentPage.Website.BlockedDomains).Contains(requestHost) ||
                BlockedExtensions.Any(url.EndsWith) ||
                e.Request.IsNavigationRequest && e.Request.RedirectChain.Length != 0)
            {
                return PuppeteerRequestAction.Abort;
            }

            return PuppeteerRequestAction.Continue;
        }

        private static bool IsFaviconRequest(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var path = uri.AbsolutePath.ToLowerInvariant();

            return path.EndsWith("/favicon.ico") ||
                   path.Contains("/favicon-") ||
                   path.Contains("/apple-touch-icon") ||
                   path.EndsWith(".ico");
        }

        private static HashSet<ResourceType>? GetAllowedResourceTypes(string? allowedResourceTypes)
        {
            if (string.IsNullOrWhiteSpace(allowedResourceTypes))
            {
                return null;
            }

            return AllowedResourceTypesCache
                .GetOrAdd(allowedResourceTypes.Trim(), ParseAllowedResourceTypes)
                .ResourceTypes;
        }

        private static HashSet<string> GetBlockedDomains(string? blockedDomains)
        {
            if (string.IsNullOrWhiteSpace(blockedDomains))
            {
                return DefaultBlockedDomains;
            }

            return BlockedDomainsCache
                .GetOrAdd(blockedDomains.Trim(), ParseBlockedDomains)
                .Domains;
        }

        private static AllowedResourceTypesCacheItem ParseAllowedResourceTypes(string allowedResourceTypes)
        {
            HashSet<ResourceType> resourceTypes = [];
            foreach (var item in allowedResourceTypes.Split(ResourceTypeSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Enum.TryParse(item, ignoreCase: true, out ResourceType resourceType))
                {
                    resourceTypes.Add(resourceType);
                }
            }

            return new AllowedResourceTypesCacheItem(resourceTypes.Count > 0 ? resourceTypes : null);
        }

        private static BlockedDomainsCacheItem ParseBlockedDomains(string blockedDomains)
        {
            HashSet<string> domains = new(StringComparer.OrdinalIgnoreCase);
            foreach (var item in blockedDomains.Split(ResourceTypeSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var domain = NormalizeDomain(item);
                if (!string.IsNullOrEmpty(domain))
                {
                    domains.Add(domain);
                }
            }

            return new BlockedDomainsCacheItem(domains);
        }

        private static string? NormalizeDomain(string value)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }

            var domain = value
                .Trim()
                .TrimStart('*')
                .TrimStart('.')
                .TrimEnd('/');

            return string.IsNullOrWhiteSpace(domain)
                ? null
                : domain;
        }

        private static PuppeteerRequestAction GetBlockedResourceAction(ResourceType resourceType)
        {
            return ImageResourceTypes.Contains(resourceType)
                ? PuppeteerRequestAction.RespondWithTransparentGif
                : PuppeteerRequestAction.Abort;
        }

        private static string GetRequestHost(string url, string defaultHost)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? requestUri)
                ? requestUri.Host
                : defaultHost;
        }

        private sealed class AllowedResourceTypesCacheItem(HashSet<ResourceType>? resourceTypes)
        {
            public HashSet<ResourceType>? ResourceTypes { get; } = resourceTypes;
        }

        private sealed class BlockedDomainsCacheItem(HashSet<string> domains)
        {
            public HashSet<string> Domains { get; } = domains;
        }
    }

    internal enum PuppeteerRequestAction
    {
        Continue,
        Abort,
        RespondWithTransparentGif,
    }
}
