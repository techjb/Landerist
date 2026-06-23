using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_orels.ES;
using System.Data;

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

        public string? ListingHtmlRemoveXPath { get; set; }

        public string? NavigationWaitSelector { get; set; }

        public string? AllowedResourceTypes { get; set; }

        public string? BlockedDomains { get; set; }

        public string? UserAgent { get; set; }

        public string? HttpRequestHeaders { get; set; }

        public string BrowserUserAgent =>
            string.IsNullOrWhiteSpace(UserAgent)
                ? Config.USER_AGENT_BROWSER
                : UserAgent.Trim();

        public bool HtmlIndexingEnabled { get; set; } = false;

        public bool UseProxy { get; set; }

        public int? MinimumRequestIntervalMilliseconds { get; set; }

        public Robots? Robots = null;

        public LanguageCode LanguageCode = LanguageCode.es;

        public CountryCode CountryCode = CountryCode.ES;

        private bool Disposed;

        public Website()
        {
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(string host) : this()
        {
            Host = host;
            LoadDataRow();
        }

        public Website(Uri mainUri) : this()
        {
            SetMainUri(mainUri);
            LoadDataRow();
        }

        private void SetMainUri(Uri mainUri)
        {
            MainUri = mainUri;
            Host = MainUri.Host;
        }

        public Website(DataRow dataRow) : this()
        {
            Load(dataRow);
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
                Robots = null;
                RobotsTxt = null;
                IndexUrlRegex = null;
                SitemapUrlRegex = null;
                ListingHtmlRemoveXPath = null;
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
