using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Parse.PageType;
using landerist_library.Tools;
using landerist_orels.ES;
using System.Data;
using System.Security.Cryptography;
using System.Text;

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

        public string? ResponseBody { get; set; }

        public PageType? PageType { get; set; }

        public string? ResponseBodyText { get; set; }


        public Website Website = new();


        public HtmlDocument? HtmlDocument = null;


        private bool Disposed;


        public Page()
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
            UriHash = CalculateHash(uri);
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
            PageType = dataRow["PageType"] is DBNull ? null :
                (PageType)Enum.Parse(typeof(PageType), dataRow["PageType"].ToString()!);
            ResponseBodyText = dataRow["ResponseBodyText"] is DBNull ? null : dataRow["ResponseBodyText"].ToString();
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

        private static string CalculateHash(Uri uri)
        {
            string text = uri.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + TABLE_PAGES + " " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, NULL, NULL, NULL, NULL)";

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
            if (!Config.STORE_RESPONSE_BODY_TEXT_IN_DATABASE)
            {
                ResponseBodyText = null;
            }

            Updated = DateTime.Now;

            string query =
                "UPDATE " + TABLE_PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[PageType] = @PageType, " +
                "[ResponseBodyText] = @ResponseBodyText " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode},
                {"PageType", PageType?.ToString()},
                {"ResponseBodyText", ResponseBodyText},
            });
        }

        public bool Update(PageType? pageType)
        {
            Updated = DateTime.Now;

            SetPageType(pageType);
            string query =
               "UPDATE " + TABLE_PAGES + " SET " +
               "[Updated] = @Updated, " +
               "[PageType] = @PageType " +
               "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"PageType", PageType?.ToString()},
            });
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
            if (HtmlDocument != null && !forceReload)
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

        public void SetResponseBodyText()
        {
            LoadHtmlDocument();
            if (HtmlDocument != null)
            {
                ResponseBodyText = HtmlToText.GetText(HtmlDocument);
            }
        }

        public Listing? GetLising()
        {
            return ES_Listings.GetListing(this, false);
        }

        public bool CanIndexContent()
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
            PageType = pageType;
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

                Website.Dispose();
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
