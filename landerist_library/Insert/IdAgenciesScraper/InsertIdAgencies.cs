﻿using landerist_library.Configuration;
using landerist_library.Tools;
using System.Data;

namespace landerist_library.Insert.IdAgenciesScraper
{
    public class InsertIdAgencies() : WebsitesInserter(true)
    {
        public static void Start()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"IdAgenciesScraper\Entrega.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "ListingExample");
            Insert(uris, true);
        }
    }
}
