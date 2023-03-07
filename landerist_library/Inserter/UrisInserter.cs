using landerist_library.Websites;
using System.Data;

namespace landerist_library.Inserter
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

        public void Insert(List<Uri> uris)
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

        public bool InsertWebsite(Uri uri)
        {
            try
            {
                if (InsertedUris.Contains(uri.ToString()))
                {
                    return false;
                }
                InsertedUris.Add(uri.ToString());
                Website website = new(uri);
                return website.Insert();
            }
            catch
            {

            }
            return false;
        }
    }
}
