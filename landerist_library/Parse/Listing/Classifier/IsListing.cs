using landerist_library.Websites;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using landerist_library.Parse.PageType;
using System.Data.Common;
using landerist_library.Configuration;

namespace landerist_library.Parse.Listing.Classifier
{
    public class IsListing
    {

        public static void Create()
        {
            Console.WriteLine("Reading Listing ..");
            DataTable dataTableListings = Pages.GetResponseBodyText(PageType.PageType.Listing);
            AddColumn(dataTableListings, true);
            Console.WriteLine("Reading NotListing ..");
            int rows = dataTableListings.Rows.Count;
            DataTable dataTableNotListings = Pages.GetResponseBodyText(PageType.PageType.Listing, rows);
            AddColumn(dataTableNotListings, false);
            var combinedDataTable = Combine(dataTableListings, dataTableNotListings);
            var tables = SplitTables(combinedDataTable);
            SaveFiles(tables);
        }

        private static void AddColumn(DataTable dataTable, bool listing)
        {
            Console.WriteLine("Adding column label ..");
            string columnName = "label";
            dataTable.Columns.Add(new DataColumn("label", typeof(int)));
            foreach (DataRow dataRow in dataTable.Rows)
            {
                dataRow[columnName] = listing ? 1 : 0;
            }
        }

        private static DataTable Combine(DataTable dataTable1, DataTable dataTable2)
        {
            Console.WriteLine("Combining tables ..");
            DataTable combinedTable = new();
            foreach (DataColumn column in dataTable1.Columns)
            {
                combinedTable.Columns.Add(column.ColumnName, column.DataType);
            }

            foreach (DataRow row in dataTable1.Rows)
            {
                combinedTable.ImportRow(row);
            }
            foreach (DataRow row in dataTable2.Rows)
            {
                combinedTable.ImportRow(row);
            }

            Random random = new();
            return combinedTable.AsEnumerable().OrderBy(r => random.Next()).CopyToDataTable();
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

        private static void SaveFiles((DataTable,  DataTable) datatables) 
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
