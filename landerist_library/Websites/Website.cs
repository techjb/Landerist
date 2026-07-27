using landerist_orels.ES;

namespace landerist_library.Websites
{
    public partial class Website : IDisposable
    {
        public Uri MainUri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public DateTime? RobotsTxtUpdated { get; set; }

        public DateTime? SitemapUpdated { get; set; }

        public string? IpAddress { get; set; }

        public DateTime? IpAddressUpdated { get; set; }

        public string? IndexUrlRegex { get; set; }

        public string? SitemapUrlRegex { get; set; }

        public string? ListingUrlRegex { get; set; }

        public string? ListingCoordinateRegex { get; set; }

        public string? ListingHtmlRemoveXPath { get; set; }

        public string? ListingUnavailableRegex { get; set; }

        public string? NavigationWaitSelector { get; set; }

        public string? AllowedResourceTypes { get; set; }

        public string? BlockedDomains { get; set; }

        public string? UserAgent { get; set; }

        public string? HttpRequestHeaders { get; set; }

        public string BrowserUserAgent =>
            string.IsNullOrWhiteSpace(UserAgent)
                ? Rules.DefaultBrowserUserAgent
                : UserAgent.Trim();

        public bool HtmlIndexingEnabled { get; set; } = false;

        public bool UseProxy { get; set; }

        public int? MinimumRequestIntervalMilliseconds { get; set; }


        public LanguageCode LanguageCode = LanguageCode.es;

        public CountryCode CountryCode = CountryCode.ES;

        private bool Disposed;

        public WebsiteRules Rules { get; }

        public Website() : this(WebsiteRules.Default)
        {
        }

        public Website(WebsiteRules rules)
        {
            ArgumentNullException.ThrowIfNull(rules);
            Rules = rules;
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(string host) : this(host, WebsiteRules.Default)
        {
        }

        public Website(string host, WebsiteRules rules) : this(rules)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            Host = host;
            MainUri = new Uri($"https://{host}");
        }

        public Website(Uri mainUri) : this(mainUri, WebsiteRules.Default)
        {
        }

        public Website(Uri mainUri, WebsiteRules rules) : this(rules)
        {
            ArgumentNullException.ThrowIfNull(mainUri);
            SetMainUri(mainUri);
        }

        private void SetMainUri(Uri mainUri)
        {
            MainUri = mainUri;
            Host = MainUri.Host;
        }

        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Host = string.Empty;
                IpAddress = null;
                RobotsTxt = null;
                IndexUrlRegex = null;
                SitemapUrlRegex = null;
                ListingCoordinateRegex = null;
                ListingHtmlRemoveXPath = null;
                ListingUnavailableRegex = null;
                NavigationWaitSelector = null;
                AllowedResourceTypes = null;
                BlockedDomains = null;
                UserAgent = null;
                HttpRequestHeaders = null;
                HtmlIndexingEnabled = false;
                UseProxy = false;
                MinimumRequestIntervalMilliseconds = null;
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
