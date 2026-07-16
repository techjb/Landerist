using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Page
    {
        private static readonly PageRepository PageRepository = new();

        private void Load(DataRow dataRow)
        {
            Host = dataRow["Host"].ToString()!;
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new Uri(uriString);
            UriHash = dataRow["UriHash"].ToString()!;
            Inserted = (DateTime)dataRow["Inserted"];
            LastScrape = dataRow["LastScrape"] is DBNull ? null : (DateTime)dataRow["LastScrape"];
            LastParseListing = dataRow.Table.Columns.Contains("LastParseListing") && dataRow["LastParseListing"] is not DBNull
                ? (DateTime)dataRow["LastParseListing"]
                : null;
            NextScrape = dataRow["NextScrape"] is DBNull ? null : (DateTime)dataRow["NextScrape"];
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            Etag = dataRow.Table.Columns.Contains("Etag") && dataRow["Etag"] is not DBNull
                ? dataRow["Etag"].ToString()
                : null;
            LastModified = dataRow["LastModified"] is DBNull ? null : dataRow["LastModified"].ToString();
            PageType = dataRow["PageType"] is DBNull ? null : ParsePageType(dataRow["PageType"].ToString()!);
            PageTypeCounter = dataRow["PageTypeCounter"] is DBNull ? null : (short)dataRow["PageTypeCounter"];
            LockedBy = dataRow["LockedBy"] is DBNull ? null : dataRow["LockedBy"].ToString();
            WaitingStatus = dataRow["WaitingStatus"] is DBNull ? null : (global::landerist_library.Pages.WaitingStatus)Enum.Parse(typeof(global::landerist_library.Pages.WaitingStatus), dataRow["WaitingStatus"].ToString()!);
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

        private static PageType ParsePageType(string pageType)
        {
            if (pageType == "HttpStatusCodeNotOK")
            {
                return landerist_library.Pages.PageType.HttpStatusCodeOtherNotOK;
            }

            return Enum.Parse<PageType>(pageType);
        }

        public DataRow? GetDataRow()
        {
            return PageRepository.GetDataRow(UriHash);
        }

        public bool Insert()
        {
            return PageRepository.Insert(new Dictionary<string, object?> {
                {"Host", Host },
                {"Uri", Uri.ToString() },
                {"UriHash", UriHash },
                {"Inserted", DateTime.Now },
                {"LastScrape", null },
                {"LastParseListing", null },
                {"NextScrape", null },
                {"HttpStatusCode", null },
                {"Etag", null },
                {"LastModified", null },
                {"PageType", null },
                {"PageTypeCounter", null },
                {"LockedBy", null },
                {"WaitingStatus", null },
                {"ListingParserInputHash", null },
                {"ListingParserInputNotChangedCounter", null },
                {"TransientErrorCounter", null },
                {"ResponseBodyZipped", null  },
                {"TokenCount", null  },
            });
        }

        public bool SetPageTypeAndNextScrape(PageType? pageType)
        {
            SetPageType(pageType);
            SetNextScrapeFromNow();
            return Update();
        }

        public bool Update()
        {
            //if (Config.IsConfigurationLocal())
            //{
            //    return true;
            //}

            var updated = PageRepository.Update(new Dictionary<string, object?> {
                {"LastScrape", LastScrape },
                {"LastParseListing", LastParseListing },
                {"NextScrape", NextScrape },
                {"HttpStatusCode", HttpStatusCode},
                {"Etag", Etag},
                {"LastModified", LastModified},
                {"PageType", PageType?.ToString()},
                {"PageTypeCounter", PageTypeCounter},
                {"LockedBy", LockedBy?.ToString()},
                {"WaitingStatus", WaitingStatus?.ToString()},
                {"ListingParserInputHash", ListingParserInputHash},
                {"ListingParserInputNotChangedCounter", ListingParserInputNotChangedCounter},
                {"TransientErrorCounter", TransientErrorCounter},
                {"ResponseBodyZipped", ResponseBodyZipped},
                {"TokenCount", TokenCount},
                {"UriHash", UriHash },
            }, out Exception? exception);

            if (!updated && exception != null)
            {
                Logs.Log.WriteError("Page.Persistence Update", "Failed to update page: " + Uri + " Message: " + exception.Message);
            }
            return updated;
        }

        public void SetLastScrape()
        {
            LastScrape = DateTime.Now;
        }

        public void SetLastParseListing()
        {
            LastParseListing = DateTime.Now;
        }

        public void SetNextScrape()
        {
            var calculationDate = LastScrape ?? Inserted;
            SetNextScrape(calculationDate);
        }

        public void SetNextScrapeFromNow()
        {
            SetNextScrape(DateTime.Now);
        }

        private void SetNextScrape(DateTime calculationDate)
        {
            NextScrape = PageNextScrapeCalculator.Calculate(this, calculationDate);
        }

        public bool UpdateNextScrape()
        {
            return PageRepository.UpdateNextScrape(UriHash, NextScrape);
        }

        public bool Delete()
        {
            bool success = PageRepository.Delete(UriHash);
            return success &&
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
