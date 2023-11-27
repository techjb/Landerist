using landerist_library.Configuration;
using System.Data;

namespace landerist_library.Parse.Listing.IsListingTest
{
    public class TestFTTC: IsListingTest
    {
        public static void Start()
        {
            var dataTable = GetListingsAndNoListings(60000);
            var tables = SplitTables(dataTable);
            SaveFiles(tables);
        }

        private static (DataTable, DataTable) SplitTables(DataTable datatable)
        {
            Console.WriteLine("Splitting tables..");

            int totalRows = datatable.Rows.Count;
            int rowsInFirstTable = (int)(totalRows * 0.7);

            DataTable firstDataTable = datatable.Clone();
            DataTable secondDataTable = datatable.Clone();

            for (int i = 0; i < rowsInFirstTable; i++)
            {
                firstDataTable.ImportRow(datatable.Rows[i]);
            }

            for (int i = rowsInFirstTable; i < totalRows; i++)
            {
                secondDataTable.ImportRow(datatable.Rows[i]);
            }

            return (firstDataTable, secondDataTable);
        }

        private static void SaveFiles((DataTable, DataTable) datatables)
        {
            SaveFile(datatables.Item1, "is_listing_training.csv");
            SaveFile(datatables.Item2, "is_listing_test.csv");
        }

        private static void SaveFile(DataTable dataTable, string fileName)
        {
            Console.WriteLine("Creating file " + fileName + " ..");
            string file = Config.CLASSIFIER_DIRECTORY + fileName;
            File.Delete(file);
            Tools.Csv.Write(dataTable, file, true);
        }
    }
}
