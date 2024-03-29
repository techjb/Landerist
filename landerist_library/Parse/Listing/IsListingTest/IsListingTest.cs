﻿using landerist_library.Websites;
using System.Data;

namespace landerist_library.Parse.Listing.IsListingTest
{
    public class IsListingTest
    {
        protected static int Processed;
        protected static int Total;
        protected static int Sucess;
        protected static int NoSucess;
        protected static int Errors;

        protected static DataTable GetListingsAndNoListings(int totalRows)
        {
            int halfRows = totalRows / 2;
            Console.WriteLine("Reading Listing ..");
            DataTable dataTableListings = Pages.GetResponseBodyText(PageType.Listing, halfRows);
            AddColumnLabel(dataTableListings, true);
            Console.WriteLine("Reading NotListing ..");
            int rows = dataTableListings.Rows.Count;
            DataTable dataTableNotListings = Pages.GetResponseBodyText(PageType.NotListingByParser, rows);
            AddColumnLabel(dataTableNotListings, false);
            return Combine(dataTableListings, dataTableNotListings);
        }

        private static void AddColumnLabel(DataTable dataTable, bool listing)
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

        protected static void OutputConsole()
        {
            var noErrors = Processed - Errors;
            var processedPercentage = Math.Round((float)Processed * 100 / Total, 2);
            var successRatio = Math.Round((float)Sucess / noErrors, 3);
            Console.WriteLine(
                "Processed: " + Processed + "/" + Total + " (" + processedPercentage + "%) " +
                "Errors: " + Errors + " " +
                "No error: " + noErrors + " " +
                "Sucess: " + Sucess + " (" + successRatio + ") " +
                "NoSucess: " + NoSucess);
        }
    }
}
