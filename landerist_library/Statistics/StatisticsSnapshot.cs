﻿using Amazon.Runtime.Internal.Transform;
using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Statistics
{
    public enum StatisticsKey
    {
        Listings,
        Media,
        PublishedListings,
        UnpublishedListings,
        Pages,
        Websites,
        UpdatedIpAddress,
        UpdatedPages,
        NeedUpdate,
        UnknownPageType,
        UpdatedWebsites,
        UpdatedRobotsTxt,
        UpdatedSitemaps,
        HttpStatusCode,
        HttpStatusCode_NULL,
    }

    public class StatisticsSnapshot
    {
        private static readonly string TABLES_STATISTICS_SNAPSHOT = "[STATISTICS_SNAPSHOT]";

        public static void TakeSnapshots()
        {
            SnapshotWebsites();
            SnapshotUpdatedRobotsTxt();
            SnapshotUpdatedSitemaps();
            SnapshotUpdatedIpAddress();
            SnapshotPages();
            SnapshotUpdatedPages();
            SnapshotNeedUpdate();
            SnapshotUnknownPageType();
            SnapshotListings();
            SnapshotPublishedListings();
            SnapshotUnPublishedListings();
            SnapshotMedia();
            SnapshotHttpStatusCode();
        }

        private static void SnapshotWebsites()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES;

            InsertDaily(StatisticsKey.Websites, query);
        }

        private static void SnapshotUpdatedRobotsTxt()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [RobotsTxtUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedRobotsTxt, query);
        }

        private static void SnapshotUpdatedSitemaps()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [SitemapUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedSitemaps, query);
        }

        private static void SnapshotUpdatedIpAddress()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [IpAddressUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedIpAddress, query);
        }

        private static void SnapshotPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.PAGES;

            InsertDaily(StatisticsKey.Pages, query);
        }

        private static void SnapshotUpdatedPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.PAGES + " " +
                "WHERE CONVERT(date, [Updated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedPages, query);
        }

        private static void SnapshotNeedUpdate()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.PAGES + " " +
                "WHERE [NextUpdate] < GETDATE()";

            InsertDaily(StatisticsKey.NeedUpdate, query);
        }

        private static void SnapshotUnknownPageType()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.PAGES + " " +
                "WHERE [PageType] IS NULL";

            InsertDaily(StatisticsKey.UnknownPageType, query);
        }

        private static void SnapshotListings()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS;

            InsertDaily(StatisticsKey.Listings, query);
        }

        private static void SnapshotPublishedListings()
        {
            SnapshotListings(StatisticsKey.PublishedListings, ListingStatus.published);
        }

        private static void SnapshotUnPublishedListings()
        {
            SnapshotListings(StatisticsKey.UnpublishedListings, ListingStatus.unpublished);
        }

        private static void SnapshotListings(StatisticsKey statisticsKey, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = '" + listingStatus.ToString() + "'";

            InsertDaily(statisticsKey, query);
        }

        private static void SnapshotMedia()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Media.TABLE_ES_MEDIA;

            InsertDaily(StatisticsKey.Media, query);
        }

        private static bool InsertDaily(StatisticsKey statisticsKey, string queryInt)
        {
            int counter = new DataBase().QueryInt(queryInt);

            string query =
                "DELETE FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [key] = @Key AND CAST([Date] AS date) = CAST(@Date AS date) " +
                "INSERT INTO " + TABLES_STATISTICS_SNAPSHOT + " " +
                "VALUES(@Date, @Key, @Counter) ";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                { "Date", DateTime.Now},
                { "Key", statisticsKey.ToString() },
                { "Counter", counter },

                });
        }

        public static DataSet GetStatistics(int lastMonths)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " ";

            DataTable dataTable = new DataBase().QueryTable(query);

            DataSet dataSet = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                string key = (string)dataRow["Key"];
                var dataTableKey = GetStatistics(key, lastMonths);
                dataSet.Tables.Add(dataTableKey);
            }
            return dataSet;
        }

        private static DataTable GetStatistics(string key, int months)
        {
            string query =
                "SELECT [Date], [Key], [Counter] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key AND " +
                "[Date] > DATEADD(MONTH, @Months, GETDATE()) " +
                "ORDER BY [Date] ASC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                { "Key", key },
                { "Months", months }
            });
        }

        public static DataTable GetTop100Statistics(string statisticsKey)
        {
            string query =
                "SELECT TOP 100 [Date], [Counter] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key " +
                "ORDER BY [Date] DESC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                { "Key", statisticsKey}
            });
        }

        

        public static void SnapshotHttpStatusCode7Days()
        {
            for(var days = -7; days <=-1; days++)
            {
                SnapshotHttpStatusCode(days);
            }
        }

        public static void SnapshotHttpStatusCode()
        {
            SnapshotHttpStatusCode(-1);
        }

        public static void SnapshotHttpStatusCode(int days)
        {
            DateTime date = DateTime.Now.AddDays(days);
            
            string query =
                "SELECT [HttpStatusCode], COUNT(*) AS [Counter] " +
                "FROM " + Websites.Pages.PAGES + " " +
                "WHERE CAST([Updated] AS date) = CAST(@Date AS date) " +
                "GROUP BY [HttpStatusCode] ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                {"Date", date}
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                short? httpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
                int counter = (int)dataRow["Counter"];
                InsertHttpStatusCode(httpStatusCode, counter, date);
            }
        }

        private static void InsertHttpStatusCode(int? httpStatusCode, int counter, DateTime date)
        {
            string key = StatisticsKey.HttpStatusCode.ToString() + "_" + (httpStatusCode?.ToString() ?? "NULL");

            string query =
                "DELETE FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key AND CAST([Date] AS date) = CAST(@Date AS date) " +
                "INSERT INTO " + TABLES_STATISTICS_SNAPSHOT + " " +
                "VALUES(@Date, @Key, @Counter) ";

            new DataBase().Query(query, new Dictionary<string, object?> {
                { "Date",date },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public static List<string> GetHttpStatusCodeKeys()
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] LIKE 'HttpStatusCode%'";

            return new DataBase().QueryListString(query);
        }

    }
}
