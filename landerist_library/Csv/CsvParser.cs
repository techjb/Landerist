using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using landerist_library.Websites;


namespace landerist_library.Csv
{
    public class CsvParser
    {
        public static void InsertWebsites()
        {
            string file = @"E:\Landerist\Csv\Base_de_datos\Excel\Pedido_completo.csv";
            Console.WriteLine("Reading " + file);
            DataTable dataTable = ReadFileWithStreamReader(file, ';');
            int inserted = 0;
            int errors = 0;
            Console.WriteLine("Inserting websites ..");
            foreach (DataRow row in dataTable.Rows)
            {
                string url = row["SITIO WEB"].ToString() ?? string.Empty;
                if (url.Equals(string.Empty))
                {
                    continue;
                }
                if (InsertWebsite(url))
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

        public static bool InsertWebsite(string url)
        {
            try
            {
                Uri uri = new(url);
                Website website = new(uri);
                return website.Insert();
            }
            catch
            {

            }
            return false;
        }

        public static DataTable ReadFileWithStreamReader(string fileName, char separator)
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
                        text = text.Substring(1, text.Length - 2);
                    }
                    dataRow[i] = text;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
    }
}
