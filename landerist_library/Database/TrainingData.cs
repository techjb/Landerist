using landerist_library.Websites;
using System.Data;
using System.IO.Compression;
using landerist_library.Parse.ListingParser;
using NetTopologySuite.Triangulate;

namespace landerist_library.Database
{
    public class TrainingData
    {
        private const string TRAINING_DATA = "[TRAINING_DATA]";


        public bool Insert(Page page)
        {
            if (!page.PageType.Equals(PageType.Listing) && !page.PageType.Equals(PageType.NotListingByParser))
            {
                return false;
            }

            string query =
                "INSERT INTO " + TRAINING_DATA + " " +
                "VALUES (@UriHash, @ResponseBodyTextHash, CONVERT(varbinary(max), @ResponseBodyZipped),  @IsListing)";
            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", page.UriHash},
                {"ResponseBodyTextHash", page.ResponseBodyTextHash},
                {"ResponseBodyZipped", page.ResponseBodyZipped },
                {"IsListing", page.PageType.Equals(PageType.Listing) },
            });
        }

        public static void CreateCsvs()
        {
            DataTable dataTable = GetDataTable(10000);
            dataTable = Parse(dataTable);
            var (train, valid, test) = DivideTable(dataTable);
            WriteFiles(train, valid, test);
        }

        private static DataTable GetDataTable(int rows)
        {
            Console.WriteLine("Reading " + rows + " rows");
            var dataTableListings = GetDataTable(rows / 2, true);
            var dataTableNotListings = GetDataTable(rows / 2, false);

            dataTableListings.Merge(dataTableNotListings);
            return dataTableListings;
        }

        private static DataTable GetDataTable(int rows, bool isListing)
        {
            string query =
               "SELECT TOP " + rows + " [ResponseBodyZipped], [IsListing] " +
               "FROM " + TRAINING_DATA + " " +
               "WHERE [IsListing] = @IsListing " +
               "ORDER BY NEWID()";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "IsListing", isListing }
            });
        }

        public static DataTable Parse(DataTable dataTable)
        {
            Console.WriteLine("Parsing " + dataTable.Rows.Count + " rows");
            DataTable parsedDataTable = new();
            parsedDataTable.Columns.Add("text", typeof(string));
            parsedDataTable.Columns.Add("label", typeof(bool));

            Parallel.ForEach(dataTable.AsEnumerable(), row =>
            {
                byte[] responseBodyZipped = (byte[])row["ResponseBodyZipped"];
                bool label = (bool)row["IsListing"];
                string responseBody = GetResponseBody(responseBodyZipped);
                var text = ParseListingUserInput.GetText(responseBody);
                lock (parsedDataTable)
                {
                    parsedDataTable.Rows.Add(text, label);
                }
            });
            return parsedDataTable;
        }

        private static string GetResponseBody(byte[] responseBodyZipped)
        {
            using var memoryStream = new MemoryStream(responseBodyZipped);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gzipStream);
            return streamReader.ReadToEnd();
        }

        public static (DataTable train, DataTable valid, DataTable test) DivideTable(DataTable dataTable)
        {
            Console.WriteLine("Dividing " + dataTable.Rows.Count + " rows into train, valid and test");
            DataTable dataTableTrain = dataTable.Clone();
            DataTable dataTableValid = dataTable.Clone();
            DataTable dataTableTest = dataTable.Clone();

            Random rnd = new();
            List<DataRow> rows = [.. dataTable.AsEnumerable().OrderBy(r => rnd.Next())];

            int total = rows.Count;
            int count90 = (int)(total * 0.90);
            int count5 = (int)(total * 0.05);

            for (int i = 0; i < total; i++)
            {
                if (i < count90)
                    dataTableTrain.ImportRow(rows[i]);
                else if (i < count90 + count5)
                    dataTableValid.ImportRow(rows[i]);
                else
                    dataTableTest.ImportRow(rows[i]);
            }

            return (dataTableTrain, dataTableValid, dataTableTest);
        }

        private static void WriteFiles(DataTable dataTableTrain, DataTable dataTableValid, DataTable dataTableTest)
        {
            Console.WriteLine("Writing files ..");
            Tools.Csv.Write(dataTableTrain, Configuration.PrivateConfig.TRAININGDATA_DIRECTORY_LOCAL + "train.csv", true);
            Tools.Csv.Write(dataTableValid, Configuration.PrivateConfig.TRAININGDATA_DIRECTORY_LOCAL + "valid.csv", true);
            Tools.Csv.Write(dataTableTest, Configuration.PrivateConfig.TRAININGDATA_DIRECTORY_LOCAL + "test.csv", true);
        }
    }
}
