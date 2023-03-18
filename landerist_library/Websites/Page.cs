﻿using System.Data;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;
using landerist_library.Scraper;

namespace landerist_library.Websites
{
    internal class Page : WebBase
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

        private HtmlDocument HtmlDocument = null;

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
            Updated = dataRow["Updated"] is DBNull? null : (DateTime)dataRow["Updated"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            ResponseBody = dataRow["ResponseBody"] is DBNull ? null : dataRow["ResponseBody"].ToString();
            IsAdvertisement = dataRow["IsAdvertisement"] is DBNull ? null : (bool)dataRow["IsAdvertisement"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + PAGES + " " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, NULL, NULL, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
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
                "UPDATE " + PAGES + " SET " +
                "[Updated] = @Updated, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[ResponseBody] = @ResponseBody, " +
                "[IsAdvertisement] = @IsAdvertisement " +
                "WHERE [UriHash] = @UriHash";

            return new Database().Query(query, new Dictionary<string, object> {
                {"UriHash", UriHash },
                {"Updated", Updated },
                {"HttpStatusCode", HttpStatusCode==null?DBNull.Value: HttpStatusCode},
                {"ResponseBody", ResponseBody==null?DBNull.Value: ResponseBody },
                {"IsAdvertisement", IsAdvertisement==null?DBNull.Value: IsAdvertisement },
            });
        }

        public bool Process(Website website)
        {
            Website = website;
            if (DownloadPage())
            {
                InsertNewPages();
                SetIsAdvertisement();
            }
            return Update();
        }

        private bool DownloadPage()
        {
            HttpStatusCode = null;
            ResponseBody = null;

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
                HttpStatusCode = (short)response.StatusCode;
                ResponseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
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
            if (HtmlDocument == null)
            {
                HtmlDocument = new();
                HtmlDocument.LoadHtml(ResponseBody);
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
                if (Website != null && !Website.CanAccess(uri))
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
            var responseBodyText = GetResponseBodyText();
            if (responseBodyText.Length < 16000)
            {
                IsAdvertisement = new ChatGPT().IsAdvertisement(responseBodyText).Result;
            }            
        }

        private string GetResponseBodyText()
        {
            string text = string.Empty;
            try
            {
                LoadHtmlDocument();
                var visibleNodes = HtmlDocument.DocumentNode.DescendantsAndSelf().Where(
                    n => n.NodeType == HtmlNodeType.Text && 
                    n.ParentNode.Name != "script" &&
                    n.ParentNode.Name != "nav" &&
                    n.ParentNode.Name != "footer" &&
                    n.ParentNode.Name != "style" && 
                    n.ParentNode.Name != "head")
                    .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml));

                text = string.Join(Environment.NewLine, visibleNodes.Select(n => n.InnerHtml.Trim()));                
            }
            catch
            {

            }
            return text;

        }


    }
}
