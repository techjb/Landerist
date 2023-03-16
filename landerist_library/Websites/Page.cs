using System.Data;
using System.Security.Cryptography;
using System.Text;

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
            });
        }

        public bool SetBodyAndStatusCode()
        {
            try
            {
                HttpClientHandler handler = new()
                {
                    AllowAutoRedirect = false
                };
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.ScraperBase.UserAgentChrome);
                HttpRequestMessage request = new(HttpMethod.Get, Uri);

                var response = client.SendAsync(request).GetAwaiter().GetResult();
                
                ResponseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                HttpStatusCode = (short)response.StatusCode;

                return Update();
            }
            catch
            {
                return false;
            }   
        }
    }
}
