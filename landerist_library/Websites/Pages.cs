﻿using System;
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
        public Pages()
        {

        }

        public List<Page> GetAll()
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " ";

            DataTable dataTable = new Database().QueryTable(query);
            return GetPages(dataTable);
        }

        public List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " " +
                "WHERE [Domain] = @Domain";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object> {
                {"Domain", website.Host }
            });

            return GetPages(dataTable);
        }

        private List<Page> GetPages(DataTable dataTable)
        {
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(dataRow);
                pages.Add(page);
            }
            return pages;
        }
    }
}
