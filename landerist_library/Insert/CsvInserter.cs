using System.Data;
using System.Text;
using System.Text.RegularExpressions;


namespace landerist_library.Insert
{
    public class CsvInserter : WebsitesInserter
    {
        public CsvInserter(bool initInsertedUris) : base(initInsertedUris)
        {
        }


        public void InsertBancodedatos_es()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + @"bancodedatos.es\Excel\Pedido_completo.csv";
            DataTable dataTable = ReadFile(file);
            var uris = ToList(dataTable, "SITIO WEB");            
            Insert(uris);
        }

        public void InsertBaseDeedatosempresas_es()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = ReadFile(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }

       
        // File must be semicolom separated
        private static DataTable ReadFile(string fileName)
        {
            Console.WriteLine("Reading " + fileName);
            DataTable dataTable = new();
            StreamReader streanReader = new(fileName, Encoding.UTF8);
            var line = streanReader.ReadLine();
            if (line == null)
            {
                return dataTable;
            }
            string[] headers = SplitCsvLine(line);

            foreach (string header in headers)
            {
                dataTable.Columns.Add(header.Replace("\"", string.Empty));
            }
            int errors = 0;
            while (!streanReader.EndOfStream)
            {
                line = streanReader.ReadLine();
                if (line == null)
                {
                    continue;
                }
                string[] rows = SplitCsvLine(line);
                if (!rows.Length.Equals(headers.Length))
                {
                    errors++;
                    continue;
                }
                DataRow dataRow = dataTable.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    var text = rows[i];
                    if (text.StartsWith("\"") && text.EndsWith("\"") && text.Length > 2)
                    {
                        text = text[1..^1];
                    }
                    dataRow[i] = text;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        static string[] SplitCsvLine(string line)
        {
            var pattern = new Regex("(?<=^|;)(\"(?:[^\"]|\"\")*\"|[^;]*)", RegexOptions.Compiled);
            var matches = pattern.Matches(line);
            var values = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                var value = matches[i].Value;
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                    value = value.Replace("\"\"", "\"");
                }
                values[i] = value;
            }
            return values;
        }

        private List<Uri> ToList(DataTable dataTable, string columnName)
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
