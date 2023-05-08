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

        public static void InserWebsites()
        {
            string file = @"E:\Landerist\Csv\Base_de_datos\Excel\Pedido_completo.csv";
            Console.WriteLine("Reading " + file);
            DataTable dataTable = ReadFile(file, ';');

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

        public static DataTable ReadFile(string fileName, char separator)
        {
            DataTable dataTable = new();

            StreamReader streanReader = new(fileName, Encoding.UTF8);
            var line = streanReader.ReadLine();
            if (line == null)
            {
                return dataTable;
            }
            string[] headers = line.Split(separator);

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
                string[] rows = Regex.Split(line, separator.ToString());
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
    }
}
