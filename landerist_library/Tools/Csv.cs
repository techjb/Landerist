using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;

namespace landerist_library.Tools
{
    public class Csv
    {
        public static DataTable ToDataTable(string file)
        {
            using var streamReader = new StreamReader(file);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                BadDataFound = null
            };
            using var csvReader = new CsvReader(streamReader, config);            
            using var csvDataReader = new CsvDataReader(csvReader);

            var dataTable = new DataTable();            
            dataTable.Load(csvDataReader);
            return dataTable;
        }

        public static void Write(DataTable dataTable, string filePath, bool addHeaders)
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
            if (addHeaders)
            {
                AddHeader(dataTable, csvWriter);
            }
            AddRows(dataTable, csvWriter);
        }

        private static void AddHeader(DataTable dataTable, CsvWriter csvWriter)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                csvWriter.WriteField(column.ColumnName);
            }
            csvWriter.NextRecord();
        }

        private static void AddRows(DataTable dataTable, CsvWriter csvWriter)
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
                    if (field.GetType() == typeof(string))
                    {
                        field = Strings.Clean((string)field);
                    }
                    csvWriter.WriteField(field);
                }
                csvWriter.NextRecord();
            }
        }
    }
}
