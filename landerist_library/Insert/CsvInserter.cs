using landerist_library.Configuration;
using landerist_library.Tools;
using System.Data;


namespace landerist_library.Insert
{
    public class CsvInserter() : WebsitesInserter(true)
    {
        public void InsertBancodedatos_es()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"bancodedatos.es\Excel\Pedido_completo.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "SITIO WEB");
            Insert(uris);
        }

        public void InsertBasededatosempresas_net()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }

        public void InsertIdAgencies()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"IdAgenciesScraper\Entrega.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "ListingExample");
            Insert(uris, true);
        }


        private static HashSet<Uri> ToList(DataTable dataTable, string columnName)
        {
            Console.WriteLine("Parsing to list ..");
            HashSet<Uri> uris = [];
            foreach (DataRow row in dataTable.Rows)
            {
                string url = row[columnName].ToString() ?? string.Empty;
                if (url.Equals(string.Empty))
                {
                    continue;
                }
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                try
                {
                    Uri uri = new(url);
                    uris.Add(uri);
                }
                catch { }
            }
            return uris;
        }
    }
}
