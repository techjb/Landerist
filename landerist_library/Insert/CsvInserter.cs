using landerist_library.Tools;
using System.Data;
using landerist_library.Configuration;


namespace landerist_library.Insert
{
    public class CsvInserter(bool initInsertedUris) : WebsitesInserter(initInsertedUris)
    {
        public static void InsertBancodedatos_es()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"bancodedatos.es\Excel\Pedido_completo.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "SITIO WEB");
            Insert(uris);
        }

        public static void InsertBasededatosempresas_net()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }

        private static List<Uri> ToList(DataTable dataTable, string columnName)
        {
            List<Uri> uris = [];
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
                    if (!uris.Contains(uri))
                    {
                        uris.Add(uri);
                    }
                }
                catch { }
            }
            return uris;
        }
    }
}
