using System.Data;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;
using landerist_library.Scraper;

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

        public bool? IsAdvertisement { get; set; }


        private Website? Website;


        private HtmlDocument? HtmlDocument = null;

        private string? ResponseBodyText = null;

        public Page(Website website) : this(website.MainUri)
        {
            Website = website;
        }

        public Page(Uri uri)
        {
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



        public Page(DataRow dataRow)
        {
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
            IsAdvertisement = dataRow["IsAdvertisement"] is DBNull ? null : (bool)dataRow["IsAdvertisement"];
        }

        public DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = new Database().QueryTable(query, new Dictionary<string, object?> {
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

            return new Database().Query(query, new Dictionary<string, object?> {
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
                "[IsAdvertisement] = @IsAdvertisement " +
                "WHERE [UriHash] = @UriHash";

            return new Database().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode},
                {"ResponseBody", ResponseBody},
                {"IsAdvertisement", IsAdvertisement },
            });
        }

        public bool Scrape(Website website)
        {
            Website = website;
            var task = Task.Run(async () => await DownloadPage());
            if (task.Result)
            {
                InsertNewPages();
                SetIsAdvertisement();
            }
            return Update();
        }

        private async Task<bool> DownloadPage()
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

        private void InsertNewPages()
        {
            try
            {
                LoadHtmlDocument();
                if (HtmlDocument == null)
                {
                    return;
                }
                var links = HtmlDocument.DocumentNode.Descendants("a")
                    .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                    .Select(a => a.Attributes["href"]?.Value)
                    .Where(href => !string.IsNullOrWhiteSpace(href))
                    .ToList();

                if (links != null)
                {
                    InsertNewLinks(links);
                }
            }
            catch
            {

            }
        }

        private void LoadHtmlDocument()
        {
            if (HtmlDocument != null)
            {
                return;
            }
            HtmlDocument = new();
            try
            {
                HtmlDocument.LoadHtml(ResponseBody);
            }
            catch
            {

            }
        }

        private void InsertNewLinks(List<string?> links)
        {
            var pages = GetPages(links);
            foreach (var page in pages)
            {
                page.Insert();
            }
        }


        private List<Page> GetPages(List<string?> links)
        {
            links = links.Distinct().ToList();
            List<Page> pages = new();
            foreach (var link in links)
            {
                if (!Uri.TryCreate(Uri, link, out Uri? uri))
                {
                    continue;
                }
                if (!uri.Host.Equals(Host) || uri.Equals(Uri))
                {
                    continue;
                }
                if (Website != null && !Website.IsPathAllowed(uri))
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
            SetResponseBodyText();
            if (ResponseBodyText != null && ResponseBodyText.Length < 16000)
            {
                IsAdvertisement = new ChatGPT().IsAdvertisement(ResponseBodyText).Result;
            }
        }

        private void SetResponseBodyText()
        {
            if (ResponseBodyText != null)
            {
                return;
            }

            try
            {
                LoadHtmlDocument();
                RemoveResponseBodyNodes();
                SetResponseBodyTextVisible();
            }
            catch
            {

            }
        }

        private void RemoveResponseBodyNodes()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var xPath =
                "//script | //nav | //footer | //style | //head | " +
                "//form | //a | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea";

            var nodesToRemove = HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private void SetResponseBodyTextVisible()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var visibleNodes = HtmlDocument.DocumentNode.DescendantsAndSelf().Where(
                   n => n.NodeType == HtmlNodeType.Text)
                   .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml));

            var visibleText = visibleNodes.Select(n => n.InnerHtml.Trim());
            ResponseBodyText = string.Join(Environment.NewLine, visibleText);
        }
    }
}
