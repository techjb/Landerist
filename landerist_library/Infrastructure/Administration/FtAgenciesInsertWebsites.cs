using landerist_library.Application.Administration;
using landerist_library.Configuration;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Data;


namespace landerist_library.Infrastructure.Administration
{
    public class FtAgenciesInsertWebsites(IWebsiteAdministrationService websites) : WebsitesInserter(true, websites)
    {
        public void Start()
        {
            string file = AppConfig.INSERT_DIRECTORY + @"FtAgencies\ListingExamples.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            dataTable.Columns[0].ColumnName = "ListingExample";
            var uris = ToList(dataTable, "ListingExample");
            Insert(uris);
        }
    }
}
