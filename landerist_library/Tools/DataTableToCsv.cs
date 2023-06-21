using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public class DataTableToCsv
    {
        public static void Convert(DataTable dataTable, string filePath, bool removeBreaklines)
        {
            using StreamWriter writer = new(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                TrimOptions = TrimOptions.Trim,
                ShouldQuote = (field) => true,
                //ShouldQuote = (field) => field.Field != null,                
                //ShouldQuote = (field) =>
                //{
                //    Console.WriteLine(field.Field);
                //    return field.Field != null;
                //}
            };

            using CsvWriter csvWriter = new(writer, config);
            AddHeader(dataTable, csvWriter);
            AddRows(dataTable, csvWriter, removeBreaklines);
        }

        private static void AddHeader(DataTable dataTable, CsvWriter csvWriter)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }

            csvWriter.NextRecord();
        }

        private static void AddRows(DataTable dataTable, CsvWriter csvWriter, bool removeBrealines)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    var field = row[i];
                    if (field is DBNull)
                    {
                        field = "null";
                    }
                    if (removeBrealines)
                    {
                        if (field.GetType() == typeof(string))
                        {
                            field = Regex.Replace(((string)field), @"\r\n?|\n", " ");
                        }
                    }
                    csvWriter.WriteField(field);
                }
                csvWriter.NextRecord();
            }
        }
    }
}
