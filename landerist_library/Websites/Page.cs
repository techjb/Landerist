using System.Data;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;

namespace landerist_library.Websites
{
    internal class Page : WebBase
    {
        public string Host { get; set; }

        public Uri Uri { get; set; }

        public string UriHash { get; set; }

        public DateTime Inserted { get; set; }

        public DateTime Updated { get; set; }

        public short? HttpStatusCode { get; set; }

        public string? ResponseBody { get; set; }

        public bool? IsAdvertisement { get; set; }


        public Page(Website website) : this(website.MainUri) { }
        public Page(Uri uri)
        {
            Host = uri.Host;
            Uri = uri;
            UriHash = CalculateHash(uri);
            Inserted = DateTime.Now;
            Updated = DateTime.Now;
        }

        private static string CalculateHash(Uri uri)
        {
            string text = uri.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public Page(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            Updated = (DateTime)dataRow["Updated"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            ResponseBody = dataRow["ResponseBody"] is DBNull ? null : dataRow["ResponseBody"].ToString();
            IsAdvertisement = dataRow["IsAdvertisement"] is DBNull ? null : (bool)dataRow["IsAdvertisement"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + PAGES + " " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @Updated, NULL, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", Inserted },
                {"Updated", Updated }
            });
        }

        public bool Update()
        {
            Updated = DateTime.Now;

            string query =
                "UPDATE " + PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[ResponseBody] = @ResponseBody " +
                "WHERE [UriHash] = @UriHash";

            return new Database().Query(query, new Dictionary<string, object> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode },
                {"ResponseBody", ResponseBody },
                {"IsAdvertisement", IsAdvertisement },
            });
        }

        public bool Process()
        {
            if (DownloadPage())
            {
                ExtractNewPages();
                SetIsAdvertisement();
            }
            return Update();
        }

        private bool DownloadPage()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.ScraperBase.UserAgentChrome);
            HttpRequestMessage request = new(HttpMethod.Get, Uri);

            try
            {
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                ResponseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                HttpStatusCode = (short)response.StatusCode;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private void ExtractNewPages()
        {
            try
            {
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(ResponseBody);

                var links = htmlDoc.DocumentNode.Descendants("a")
                    .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                    .Select(a => a.Attributes["href"]?.Value)
                    .Where(href => !string.IsNullOrWhiteSpace(href))
                    .ToList();

                InsertNewPages(links);
            }
            catch(Exception ex)
            {

            }
        }

        private void InsertNewPages(List<string> links)
        {
            var pages = GetPages(links);
            foreach(var page in pages )
            {
                page.Insert();
            }
        }
        

        private List<Page> GetPages(List<string> links)
        {
            links = links.Distinct().ToList();
            List<Page> pages = new();
            foreach (var link in links)
            {
                var uri = new Uri(Uri, link);
                if (!uri.Host.Equals(Host)
                    || uri.Equals(Uri))
                {
                    continue;
                }
                Page page = new(uri);                
                pages.Add(page);
            }
            return pages;
        }

        private void SetIsAdvertisement()
        {

        }
    }
}
