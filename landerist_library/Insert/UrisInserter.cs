using landerist_library.Websites;
using System.Data;

namespace landerist_library.Insert
{
    public class UrisInserter
    {
        private readonly HashSet<string> InsertedUris;

        public UrisInserter()
        {
            InsertedUris = Websites.Websites.GetUris();
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
                    InitWebsite(uri);
                }
                else
                {
                    errors++;
                }
            }
            Console.WriteLine("Inserted: " + inserted + " Error: " + errors);
        }

        public bool InsertWebsite(Uri uri)
        {
            try
            {
                if (BlockedDomains.IsBlocked(uri))
                {
                    return false;
                }

                if (InsertedUris.Contains(uri.ToString()))
                {
                    return false;
                }
                
                Website website = new()
                {
                    MainUri = uri,
                    Host = uri.Host,
                };
                if (website.Insert())
                {
                    InsertedUris.Add(uri.ToString());
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }

        private static void InitWebsite(Uri uri)
        {
            Website website = new(uri);

            website.SetHttpStatusCode();
            website.SetRobotsTxt();
            website.SetIpAddress();
            website.InsertMainPage();
        }
    }
}
