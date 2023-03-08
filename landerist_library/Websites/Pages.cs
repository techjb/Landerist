using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Websites
{
    internal class Pages : WebBase
    {
        private readonly Website Website;

        private readonly List<Page> ListPages = new();


        public Pages(Website website)
        {
            Website = website;
        }

        private void LoadPages()
        {
            string query =
                "SELECT * FROM " + PAGES + " " +
                "WHERE [Domain] = @Domain";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object> {
                {"Domain", Website.Domain }
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(dataRow);
                ListPages.Add(page);
            }
        }
    }
}
