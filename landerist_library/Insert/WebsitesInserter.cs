using landerist_library.Websites;
using System.Data;

namespace landerist_library.Insert
{
    public class WebsitesInserter
    {
        private readonly static HashSet<Uri> InsertedUris = new();

        public WebsitesInserter(bool initInsertedUris)
        {
            if (initInsertedUris)
            {
                InitInsertedUris();
            }
        }

        private static void InitInsertedUris()
        {
            var urls = Websites.Websites.GetUrls();
            foreach (var url in urls)
            {
                Uri uri = new(url);
                InsertedUris.Add(uri);
            }
        }

        public void FromCsv()
        {
            string file = @"E:\Landerist\Csv\Base_de_datos\Excel\Pedido_completo.csv";
            Console.WriteLine("Reading " + file);
            DataTable dataTable = CsvReader.ReadFile(file, ';');

            List<Uri> uris = new();
            foreach (DataRow row in dataTable.Rows)
            {
                string url = row["SITIO WEB"].ToString() ?? string.Empty;
                if (url.Equals(string.Empty))
                {
                    continue;
                }
                try
                {
                    Uri uri = new(url);
                    if (!uris.Contains(uri))
                    {
                        uris.Add(uri);
                    }
                }
                catch { }
            }
            Insert(uris);
        }

        public void RemoveAndInsert(Uri uri)
        {
            Website website = new(uri);
            RemoveAndInsert(website);
        }

        public void RemoveAndInsert(Website website)
        {
            website.Remove();
            Insert(website.MainUri);
        }


        public void Insert(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                Insert(uri);
            }
        }

        public void Insert(Uri uri)
        {
            var list = new List<Uri>()
            {
                uri
            };
            Insert(list);
        }

        public void Insert(List<Uri> uris)
        {
            HashSet<Uri> hashSet = new(uris);
            Insert(hashSet);
        }

        private void Insert(HashSet<Uri> uris)
        {
            int inserted = 0;
            int errors = 0;
            int total = uris.Count;

            Console.WriteLine("Inserting " + total + " websites ..");

            foreach (Uri uri in uris)
            {
                if (InsertWebsite(uri))
                {
                    inserted++;
                }
                else
                {
                    errors++;
                }
            }
            Console.WriteLine("Inserted: " + inserted + " Error: " + errors);
        }

        private static bool InsertWebsite(Uri uri)
        {
            try
            {
                if (BlockedDomains.IsBlocked(uri))
                {
                    return false;
                }

                if (InsertedUris.Contains(uri))
                {
                    return false;
                }

                var website = Websites.Websites.GetWebsite(uri.Host);
                if (website != null)
                {
                    return false;
                }

                website = new()
                {
                    MainUri = uri,
                    Host = uri.Host,
                };

                return InsertWebsite(website);
            }
            catch
            {

            }
            return false;
        }

        private static bool InsertWebsite(Website website)
        {
            if (!website.SetMainUriAndStatusCode())
            {
                return false;
            }
            if (InsertedUris.Contains(website.MainUri))
            {
                return false;
            }
            if (!website.SetRobotsTxt())
            {
                return false;
            }
            if (!website.SetIpAddress())
            {
                return false;
            }
            if (!website.Insert())
            {
                return false;
            }
            website.InsertMainPage();
            website.InsertPagesFromSiteMap();

            InsertedUris.Add(website.MainUri);
            
            return true;
        }
    }
}
