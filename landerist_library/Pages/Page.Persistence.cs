using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Page
    {
        private void Load(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            LastScrape = dataRow["LastScrape"] is DBNull ? null : (DateTime)dataRow["LastScrape"];
            NextScrape = dataRow["NextScrape"] is DBNull ? null : (DateTime)dataRow["NextScrape"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            Etag = dataRow.Table.Columns.Contains("Etag") && dataRow["Etag"] is not DBNull
                ? dataRow["Etag"].ToString()
                : null;
            PageType = dataRow["PageType"] is DBNull ? null : (PageType)Enum.Parse(typeof(PageType), dataRow["PageType"].ToString()!);
            PageTypeCounter = dataRow["PageTypeCounter"] is DBNull ? null : (short)dataRow["PageTypeCounter"];
            ListingStatus = dataRow["ListingStatus"] is DBNull ? null : (ListingStatus)Enum.Parse(typeof(ListingStatus), dataRow["ListingStatus"].ToString()!);
            LockedBy = dataRow["LockedBy"] is DBNull ? null : dataRow["LockedBy"].ToString();
            WaitingStatus = dataRow["WaitingStatus"] is DBNull ? null : (WaitingStatus)Enum.Parse(typeof(WaitingStatus), dataRow["WaitingStatus"].ToString()!);
            ListingParserInputHash = dataRow["ListingParserInputHash"] is DBNull ? null : dataRow["ListingParserInputHash"].ToString();
            ListingParserInputNotChangedCounter = dataRow.Table.Columns.Contains("ListingParserInputNotChangedCounter") && dataRow["ListingParserInputNotChangedCounter"] is not DBNull
                ? (short)dataRow["ListingParserInputNotChangedCounter"]
                : null;
            TransientErrorCounter = dataRow.Table.Columns.Contains("TransientErrorCounter") && dataRow["TransientErrorCounter"] is not DBNull
                ? (short)dataRow["TransientErrorCounter"]
                : null;
            ResponseBodyZipped = dataRow["ResponseBodyZipped"] is DBNull ? null : (byte[])dataRow["ResponseBodyZipped"];
            TokenCount = dataRow["TokenCount"] is DBNull ? null : (int?)dataRow["TokenCount"];
        }

        public DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });

            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }
            return null;
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Pages.PAGES + " (" +
                "[Host], [Uri], [UriHash], [Inserted], [LastScrape], [NextScrape], [HttpStatusCode], [Etag], [PageType], " +
                "[PageTypeCounter], [ListingStatus], [LockedBy], [WaitingStatus], [ListingParserInputHash], " +
                "[ListingParserInputNotChangedCounter], [TransientErrorCounter], [ResponseBodyZipped], [TokenCount]) " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @LastScrape, @NextScrape, @HttpStatusCode, @Etag, @PageType, " +
                "@PageTypeCounter, @ListingStatus, @LockedBy, @WaitingStatus, @ListingParserInputHash, " +
                "@ListingParserInputNotChangedCounter, @TransientErrorCounter, CONVERT(varbinary(max), @ResponseBodyZipped), @TokenCount)";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", DateTime.Now },
                {"LastScrape", null },
                {"NextScrape", null },
                {"HttpStatusCode", null },
                {"Etag", null },
                {"PageType", null },
                {"PageTypeCounter", null },
                {"ListingStatus", null },
                {"LockedBy", null },
                {"WaitingStatus", null },
                {"ListingParserInputHash", null },
                {"ListingParserInputNotChangedCounter", null },
                {"TransientErrorCounter", null },
                {"ResponseBodyZipped", null  },
                {"TokenCount", null  },
            });
            return sucess;
        }

        public bool SetPageTypeAndNextScrape(PageType? pageType)
        {
            SetPageType(pageType);
            SetNextScrape(DateTime.Now);
            return Update();
        }

        public bool Update()
        {
            //if (Config.IsConfigurationLocal())
            //{
            //    return true;
            //}

            string query =
                "UPDATE " + Pages.PAGES + " SET " +
                "[LastScrape] = @LastScrape, " +
                "[NextScrape] = @NextScrape, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[Etag] = @Etag, " +
                "[PageType] = @PageType, " +
                "[PageTypeCounter] = @PageTypeCounter, " +
                "[ListingStatus] = @ListingStatus, " +
                "[LockedBy] = @LockedBy, " +
                "[WaitingStatus] = @WaitingStatus, " +
                "[ListingParserInputHash] = @ListingParserInputHash, " +
                "[ListingParserInputNotChangedCounter] = @ListingParserInputNotChangedCounter, " +
                "[TransientErrorCounter] = @TransientErrorCounter, " +
                "[ResponseBodyZipped] = CASE WHEN @ResponseBodyZipped IS NULL THEN NULL ELSE CONVERT(varbinary(max), @ResponseBodyZipped) END," +
                "[TokenCount] = @TokenCount " +
                "WHERE [UriHash] = @UriHash";

            var sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"LastScrape", LastScrape },
                {"NextScrape", NextScrape },
                {"HttpStatusCode", HttpStatusCode},
                {"Etag", Etag},
                {"PageType", PageType?.ToString()},
                {"PageTypeCounter", PageTypeCounter},
                {"ListingStatus", ListingStatus?.ToString()},
                {"LockedBy", LockedBy?.ToString()},
                {"WaitingStatus", WaitingStatus?.ToString()},
                {"ListingParserInputHash", ListingParserInputHash},
                {"ListingParserInputNotChangedCounter", ListingParserInputNotChangedCounter},
                {"TransientErrorCounter", TransientErrorCounter},
                {"ResponseBodyZipped", ResponseBodyZipped},
                {"TokenCount", TokenCount},
                {"UriHash", UriHash },
            });

            if (!sucess)
            {
                Logs.Log.WriteError("Page Update", "Failed to update page: " + Uri);
            }
            return sucess;
        }

        public void SetLastScrape()
        {
            LastScrape = DateTime.Now;
        }

        public void SetNextScrape()
        {
            var calculationDate = LastScrape ?? Inserted;
            SetNextScrape(calculationDate);
        }

        private void SetNextScrape(DateTime calculationDate)
        {
            NextScrape = PageNextScrapeCalculator.Calculate(this, calculationDate);
        }

        public bool UpdateNextScrape()
        {
            string query =
               "UPDATE " + Pages.PAGES + " SET " +
               "[NextScrape] = @NextScrape " +
               "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash },
                {"NextScrape", NextScrape },
            });
        }

        public bool Delete()
        {
            string query =
                "DELETE FROM " + Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", UriHash }
            });
            return sucess &&
                ES_Listings.Delete(UriHash) &&
                ES_Media.Delete(UriHash) &&
                ES_Sources.Delete(UriHash);
        }

        public bool DeleteListing()
        {
            var listing = ES_Listings.GetListing(this, false, false);
            if (listing != null)
            {
                if (ES_Listings.Delete(listing))
                {
                    ES_Media.Delete(listing);
                    ES_Sources.Delete(listing);
                    return true;
                }
            }
            return false;
        }
    }
}
