using System.Data;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Parse;

namespace landerist_library.Websites
{
    public class Page : Pages
    {
        public string Host { get; set; }

        public Uri Uri { get; set; }

        public string UriHash { get; set; }

        public DateTime Inserted { get; set; }

        public DateTime? Updated { get; set; }

        public short? HttpStatusCode { get; set; }

        public string? ResponseBody { get; set; }

        public bool? IsListing { get; set; }


        public Website Website;


        public HtmlDocument? HtmlDocument = null;

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

        private void Load(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            Updated = dataRow["Updated"] is DBNull ? null : (DateTime)dataRow["Updated"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            ResponseBody = dataRow["ResponseBody"] is DBNull ? null : dataRow["ResponseBody"].ToString();
            IsListing = dataRow["IsListing"] is DBNull ? null : (bool)dataRow["IsListing"];
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

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", Inserted }
            });
        }

        public bool Update()
        {
            Updated = DateTime.Now;

            string query =
                "UPDATE " + TABLE_PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[ResponseBody] = @ResponseBody, " +
                "[IsListing] = @IsListing " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode},
                {"ResponseBody", ResponseBody},
                {"IsListing", IsListing },
            });
        }

        public bool CanScrape()
        {
            if (!Website.IsUriAllowed(Uri))
            {
                return false;
            }
            return true;
        }

        public bool Scrape()
        {
            var task = Task.Run(async () => await Download());
            if (task.Result)
            {
                InsertPages();
                GetListing();
            }
            return Update();
        }

        private async Task<bool> Download()
        {
            HttpStatusCode = null;
            ResponseBody = null;

            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);

            HttpRequestMessage request = new(HttpMethod.Get, Uri);
            try
            {
                var response = await client.SendAsync(request);
                HttpStatusCode = (short)response.StatusCode;
                ResponseBody = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private void InsertPages()
        {
            LoadHtmlDocument();
            if (HtmlDocument == null || Website == null)
            {
                return;
            }
            var uris = new Indexer(this).GetUris();
            Insert(Website, uris);
        }

        public void LoadHtmlDocument()
        {
            if (HtmlDocument != null)
            {
                return;
            }
            try
            {
                HtmlDocument = new();
                HtmlDocument.LoadHtml(ResponseBody);
            }
            catch
            {
                HtmlDocument = null;
            }
        }

        private void GetListing()
        {
            if (!CanRequestListing())
            {
                return;
            }
            var listingParser = new ListingParser(this).GetListing();
            IsListing = listingParser.Item1;
            var listing = listingParser.Item2;
            if (listing != null)
            {
                new ES_Listings().Insert(listing);
            }
        }

        private bool CanRequestListing()
        {
            return !IsMainPage();
        }

        public bool IsMainPage()
        {
            if (Website == null)
            {
                return false;
            }
            return Uri.Equals(Website.MainUri);
        }

        public bool LanguageIs(string language)
        {
            LoadHtmlDocument();
            if (HtmlDocument != null)
            {
                var htmlNode = HtmlDocument.DocumentNode.SelectSingleNode("//html");
                if (htmlNode != null)
                {
                    var langAttribute = htmlNode.Attributes["lang"];
                    if (langAttribute != null)
                    {
                        string value = langAttribute.Value;
                        if (value.Contains("-"))
                        {
                            value = value.Split('-')[0];
                        }
                        if (!value.Equals(language, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }                        
                    }
                }
            }
            return true;
        }
    }
}
