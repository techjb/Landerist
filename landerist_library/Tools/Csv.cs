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
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                BadDataFound = null
            };            
            using var csvReader = new CsvReader(streamReader, csvConfiguration);            
            using var csvDataReader = new CsvDataReader(csvReader);

            var dataTable = new DataTable();            
            dataTable.Load(csvDataReader);
            return dataTable;
        }

        public static void Write(DataTable dataTable, string fileName, bool addHeaders)
        {
            using StreamWriter streamWriter = new(fileName);
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                TrimOptions = TrimOptions.Trim,
                ShouldQuote = (field) => true,              
            };

            using CsvWriter csvWriter = new(streamWriter, csvConfiguration);
            if (addHeaders)
            {
                AddHeader(dataTable, csvWriter);
            }
            AddRows(dataTable, csvWriter);
        }

        public static void Write(HashSet<string> data, string filename)
        {
            try
            {
                using var streamWriter = new StreamWriter(filename);
                using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
                foreach (var item in data)
                {
                    csvWriter.WriteField(item);
                    csvWriter.NextRecord();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error writting the file: " + exception.Message);
            }
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
                    csvWriter.WriteField(field);
                }
                csvWriter.NextRecord();
            }
        }
    }
}
