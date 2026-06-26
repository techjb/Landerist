using HtmlAgilityPack;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Pages
{

    public partial class Page : IDisposable
    {
        public string Host { get; set; } = string.Empty;

        public Uri Uri { get; set; } = new Uri("about:blank");

        public string UriHash { get; set; } = string.Empty;

        public DateTime Inserted { get; set; }

        public DateTime? LastScrape { get; set; }

        public DateTime? LastParseListing { get; set; }

        public DateTime? NextScrape { get; set; }

        public short? HttpStatusCode { get; set; }

        public string? RedirectUrl { get; set; } = null;

        public string? Etag { get; set; } = null;

        public string? LastModified { get; set; } = null;

        public PageType? PageType { get; private set; }

        public short? PageTypeCounter { get; private set; }

        private ListingStatus? ListingStatus { get; set; }

        public string? LockedBy { get; set; }

        public WaitingStatus? WaitingStatus { get; private set; }

        private string? ResponseBody { get; set; }

        public string? ListingParserInput { get; set; }

        public string? ListingParserInputHash { get; set; }

        public short? ListingParserInputNotChangedCounter { get; private set; }

        public short? TransientErrorCounter { get; private set; }

        public byte[]? ResponseBodyZipped { get; private set; }

        public int? TokenCount { get; set; } = null;

        public bool ListingParserInputNotChanged { get; set; } = false;

        public byte[]? Screenshot { get; set; }

        private bool EtagNotChanged { get; set; } = false;

        private bool HasComparableEtag { get; set; } = false;

        private bool LastModifiedNotChanged { get; set; } = false;

        private bool HasComparableLastModified { get; set; } = false;

        private const string RemaxInvalidCanonicalPath = "/buscador-de-inmuebles/todos/todos/todos/todos/todos/todos";


        private HtmlDocument? HtmlDocument = null;


        private string? OriginalOuterHtml = null;

        public Website Website = new();


        private bool Disposed;

        public Page(string url) : this(new Uri(url))
        {

        }

        public Page(Uri uri) : this(Websites.Websites.GetWebsite(uri.Host), uri)
        {

        }

        public Page(Website website) : this(website, website.MainUri)
        {

        }

        public Page(Website website, Uri uri)
        {
            Website = website;
            Host = uri.Host;
            Uri = uri;
            UriHash = GetUriHash();
            Inserted = DateTime.Now;

            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        public string GetUriHash()
        {
            var uriString = Uri.ToString();
            return Strings.GetHash(uriString);
        }

        public Page(Website website, DataRow dataRow)
        {
            Website = website;
            Load(dataRow);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Host = string.Empty;
                UriHash = string.Empty;
                HtmlDocument = null;
                ResponseBody = null;
                ListingParserInput = null;
                ListingParserInputHash = null;
                Etag = null;
                LastModified = null;
                ResponseBodyZipped = null;
                Screenshot = null;
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }

    }
}
