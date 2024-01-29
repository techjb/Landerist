using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Websites
{

    public class Page : Pages, IDisposable
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


        public Website Website = new();


        public HtmlDocument? HtmlDocument = null;


        private bool Disposed;

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
                "FROM " + TABLE_PAGES + " " +
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
                "INSERT INTO " + TABLE_PAGES + " " +
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
                "UPDATE " + TABLE_PAGES + " SET " +
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
                "DELETE FROM " + TABLE_PAGES + " " +
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

        public void LoadHtmlDocument(bool forceReload = false)
        {
            if (string.IsNullOrEmpty(ResponseBody) || (HtmlDocument != null && !forceReload))
            {
                return;
            }
            try
            {
                HtmlDocument = new();
                HtmlDocument.LoadHtml(ResponseBody);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(Uri, exception);
                HtmlDocument = null;
            }
        }

        public bool IsMainPage()
        {
            if (Website == null)
            {
                return false;
            }
            return Uri.Equals(Website.MainUri);
        }

        public void InitializeResponseBody()
        {
            ResponseBody = null;
            ResponseBodyText = null;
        }
        public bool ResponseBodyIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(ResponseBody);
        }

        public void SetResponseBody(string? responseBody)
        {
            ResponseBody = responseBody;
        }

        public void SetResponseBodyText()
        {
            LoadHtmlDocument();
            if (HtmlDocument != null)
            {
                ResponseBodyText = HtmlToText.GetText(HtmlDocument);
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

        public Listing? GetListing(bool loadMedia)
        {
            return ES_Listings.GetListing(this, loadMedia);
        }

        public bool ContainsListing()
        {
            var listing = GetListing(false);
            return listing is not null;
        }

        public bool IsIndexable()
        {
            return !ContainsMetaRobots("noindex");
        }

        public bool CanIndexImages()
        {
            return !ContainsMetaRobots("noimageindex");
        }

        public bool CanFollowLinks()
        {
            return !ContainsMetaRobots("noindex");
        }

        private bool ContainsMetaRobots(string content)
        {
            LoadHtmlDocument();
            if (HtmlDocument != null)
            {
                var node = HtmlDocument.DocumentNode.SelectSingleNode("//meta[@name='robots']");
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
