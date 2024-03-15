using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Websites
{

    public class Page : IDisposable
    {
        public string Host { get; set; } = string.Empty;

        public Uri Uri { get; set; } = new Uri("about:blank");

        public string UriHash { get; set; } = string.Empty;

        public DateTime Inserted { get; set; }

        public DateTime? Updated { get; set; }

        public short? HttpStatusCode { get; set; }

        public PageType? PageType { get; private set; }

        public short? PageTypeCounter { get; private set; }

        private string? ResponseBody { get; set; }

        public string? ResponseBodyText { get; set; }

        public string? ResponseBodyTextHash { get; set; }

        public bool ResponseBodyTextHasChanged { get; set; } = false;


        private HtmlDocument? HtmlDocument = null;


        private string? OriginalOuterHtml = null;


        public Website Website = new();




        private bool Disposed;

        public Page(string url) : this(new Uri(url))
        {

        }

        public Page(Uri uri) : this(Websites.GetWebsite(uri.Host), uri)
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
            UriHash = Strings.GetHash(uri.ToString());
            Inserted = DateTime.Now;
            Updated = DateTime.Now;

            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        public Page(Website website, DataRow dataRow)
        {
            Website = website;
            Load(dataRow);
        }

        public Page(DataRow dataRow)
        {
            Load(dataRow);
            Website = Websites.GetWebsite(this);
        }

        private void Load(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            Updated = dataRow["Updated"] is DBNull ? null : (DateTime)dataRow["Updated"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            PageType = dataRow["PageType"] is DBNull ? null : (PageType)Enum.Parse(typeof(PageType), dataRow["PageType"].ToString()!);
            PageTypeCounter = dataRow["PageTypeCounter"] is DBNull ? null : (short)dataRow["PageTypeCounter"];
            ResponseBodyTextHash = dataRow["ResponseBodyTextHash"] is DBNull ? null : dataRow["ResponseBodyTextHash"].ToString();
        }

        public DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.TABLE_PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });

            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }
            return null;
        }


        public bool Insert()
        {
            string query =
                "INSERT INTO " + Pages.TABLE_PAGES + " " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, NULL, NULL, NULL, NULL, NULL)";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", Inserted }
            });
            if (sucess)
            {
                Website.IncreaseNumPages();
            }
            return sucess;
        }

        public bool Update()
        {
            Updated = DateTime.Now;

            string query =
                "UPDATE " + Pages.TABLE_PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[PageType] = @PageType, " +
                "[PageTypeCounter] = @PageTypeCounter, " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode},
                {"PageType", PageType?.ToString()},
                {"PageTypeCounter", PageTypeCounter},
                {"ResponseBodyTextHash", ResponseBodyTextHash},
            });
        }

        public bool Update(PageType? pageType)
        {
            SetPageType(pageType);
            return Update();
        }

        public bool Delete()
        {
            string query =
                "DELETE FROM " + Pages.TABLE_PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });
            if (sucess)
            {
                Website.DecreaseNumPages();
                ES_Listings.Delete(UriHash);
                ES_Media.Delete(UriHash);
            }
            return sucess;
        }

        public bool DeleteListing()
        {
            var listing = ES_Listings.GetListing(this, false);
            if (listing != null)
            {
                if (ES_Listings.Delete(listing))
                {
                    ES_Media.Delete(listing);
                    Website.DecreaseNumListings();
                    return true;
                }
            }
            return false;
        }


        public bool CanScrape()
        {
            if (!Website.IsAllowedByRobotsTxt(Uri))
            {
                return false;
            }
            var crawlDelay = Website.CrawlDelay();
            if (crawlDelay > Config.MAX_CRAW_DELAY_SECONDS)
            {
                return false;
            }
            return true;
        }

        public HtmlDocument? GetHtmlDocument()
        {
            if (HtmlDocument != null && OriginalOuterHtml != null)
            {
                string currentOuterHtml = HtmlDocument.DocumentNode.OuterHtml;
                if (OriginalOuterHtml.Equals(currentOuterHtml))
                {
                    return HtmlDocument;
                }
            }
            if (!string.IsNullOrEmpty(ResponseBody))
            {
                HtmlDocument = null;
                OriginalOuterHtml = null;

                try
                {
                    HtmlDocument = new();
                    HtmlDocument.LoadHtml(ResponseBody);
                    OriginalOuterHtml = HtmlDocument.DocumentNode.OuterHtml;
                    return HtmlDocument;
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteLogErrors("Page GetHtmlDocument", Uri, exception);
                }
            }
            return null;
        }

        public bool MainPage()
        {
            if (Website == null)
            {
                return false;
            }
            return Uri.Equals(Website.MainUri);
        }

        public bool ResponseBodyIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(ResponseBody);
        }

        public void SetResponseBodyAndStatusCode(string? responseBody, short? httpStatusCode)
        {
            ResponseBody = responseBody;
            ResponseBodyText = null;
            HttpStatusCode = httpStatusCode;
        }

        public void SetResponseBodyText()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                ResponseBodyText = HtmlToText.GetText(htmlDocument);
                SetResponseBodyTextHash();
            }
        }

        public void SetResponseBodyTextHash()
        {
            if (ResponseBodyText is null)
            {
                return;
            }
            string responseBodyTextHash = Strings.GetHash(ResponseBodyText);
            ResponseBodyTextHasChanged = ResponseBodyTextHash == null || !responseBodyTextHash.Equals(ResponseBodyTextHash);
            ResponseBodyTextHash = responseBodyTextHash;
        }

        public bool ResponseBodyTextHasNotChanged()
        {
            return
                !ResponseBodyTextHasChanged &&
                PageType != null &&
                !PageType.Equals(landerist_library.Websites.PageType.MayBeListing) &&
                Config.IsConfigurationProduction();
        }

        public bool ResponseBodyTextIsError()
        {
            if (ResponseBodyText == null)
            {
                return false;
            }
            return
                ResponseBodyText.StartsWith("Not found", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("no existe", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("algo salió mal", StringComparison.OrdinalIgnoreCase) ||
                ResponseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }

        public bool ResponseBodyTextIsTooLarge()
        {
            if (ResponseBodyText is null)
            {
                return false;
            }
            return ResponseBodyText.Length > Config.MAX_RESPONSEBODYTEXT_LENGTH;
        }

        public bool ResponseBodyTextIsTooShort()
        {
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                return true;
            }
            return ResponseBodyText.Length < Config.MIN_RESPONSEBODYTEXT_LENGTH;
        }

        public bool ReponseBodyTextRepeatedInHost()
        {
            if (string.IsNullOrEmpty(ResponseBodyText))
            {
                return false;
            }

            string query =
                "IF EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + Pages.TABLE_PAGES + " " +
                "   WHERE [HOST] = @Host AND " +
                "   [UriHash] <> @UriHash AND " +
                "   [ResponseBodyTextHash] = @ResponseBodyTextHash) " +
                "SELECT 'true' " +
                "ELSE " +
                "SELECT 'false' ";

            return new DataBase().QueryBool(query, new Dictionary<string, object?> {
                {"Host", Host},
                {"UriHash", UriHash },
                {"ResponseBodyTextHash", ResponseBodyTextHash },
            });
        }


        public Listing? GetListing(bool loadMedia)
        {
            return ES_Listings.GetListing(this, loadMedia);
        }

        public bool ContainsListing()
        {
            var listing = GetListing(false);
            return listing is not null;
        }

        public bool ContainsMetaRobotsNoIndex()
        {
            return ContainsMetaRobots("noindex");
        }

        public bool ContainsMetaRobotsNoFollow()
        {
            return ContainsMetaRobots("nofollow");
        }

        public bool ContainsMetaRobotsNoImageIndex()
        {
            return ContainsMetaRobots("noimageindex");
        }

        public bool NotCanonical()
        {
            var canonicalUri = GetCanonicalUri();
            if (canonicalUri != null)
            {
                return !Uri.Equals(canonicalUri);
            }
            return false;
        }

        public bool IncorrectLanguage()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return !LanguageValidator.IsValidLanguageAndCountry(Website, lang.Value);
                    }
                }
            }
            return false;
        }

        public Uri? GetCanonicalUri()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("href", "");
                    return new Indexer(this).GetUri(contentAttribute);
                }
            }
            return null;
        }

        private bool ContainsMetaRobots(string content)
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='robots']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(contentAttribute))
                    {
                        var contents = contentAttribute.Split(',');
                        foreach (var item in contents)
                        {
                            if (item.Equals(content) || item.Equals("none"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void SetPageType(PageType? pageType)
        {
            if (PageType == pageType)
            {
                IncreasePageTypeCounter();
            }
            else
            {
                PageTypeCounter = 1;
            }

            PageType = pageType;
        }

        private void IncreasePageTypeCounter()
        {
            if (PageTypeCounter is null)
            {
                PageTypeCounter = 1;
                return;
            }
            if (PageTypeCounter >= Config.MAX_PAGETYPE_COUNTER)
            {
                return;
            }
            PageTypeCounter = (short)(PageTypeCounter + 1);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Host = string.Empty;
                UriHash = string.Empty;
                HtmlDocument = null;
                ResponseBody = null;
                ResponseBodyText = null;
                ResponseBodyTextHash = null;

                Website.Dispose();
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
