using landerist_library.Tools;
using System.Data;


namespace landerist_library.Insert
{
    public class CsvInserter : WebsitesInserter
    {
        public CsvInserter(bool initInsertedUris) : base(initInsertedUris)
        {
        }


        public static void InsertBancodedatos_es()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + @"bancodedatos.es\Excel\Pedido_completo.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "SITIO WEB");
            Insert(uris);
        }

        public static void InsertBasededatosempresas_net()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }

        private static List<Uri> ToList(DataTable dataTable, string columnName)
        {
            List<Uri> uris = new();
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
