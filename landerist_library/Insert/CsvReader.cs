using System.Data;
using System.Text;
using System.Text.RegularExpressions;


namespace landerist_library.Insert
{
    public class CsvReader
    {
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
