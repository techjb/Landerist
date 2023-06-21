using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;

namespace landerist_library.Tools
{
    public class DataTableToCsv
    {
        public static void Convert(DataTable table, string filePath)
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

            foreach (DataColumn column in table.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }

            csvWriter.NextRecord();

            foreach (DataRow row in table.Rows)
            {
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var field = row[i];
                    if (field is DBNull)
                    {
                        field = "null";
                    }
                    csvWriter.WriteField(field);
                }
                csvWriter.NextRecord();
            }
        }
    }
}
