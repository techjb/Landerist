using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Statistics;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class HostStatisticsRepository
    {
        private readonly IDatabase? _database;

        public HostStatisticsRepository()
        {
        }

        public HostStatisticsRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database ?? new DataBase();

        public int CountPages(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            return QueryHostInt(query, host);
        }

        public int CountInsertedYesterday(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "AND CONVERT(date, [Inserted]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryHostInt(query, host);
        }

        public int CountLastScrapeYesterday(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "AND CONVERT(date, [LastScrape]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryHostInt(query, host);
        }

        public int CountListings(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host";

            return QueryHostInt(query, host);
        }

        public int CountListings(string host, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host AND [listingStatus] = @ListingStatus";

            return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        public DataTable GetHttpStatusCodeCounts(string host)
        {
            string query =
                "SELECT [HttpStatusCode], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "GROUP BY [HttpStatusCode]";

            return QueryHostTable(query, host);
        }

        public DataTable GetPageTypeCounts(string host)
        {
            string query =
                "SELECT [PageType], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "AND [PageType] IS NOT NULL " +
                "GROUP BY [PageType]";

            return QueryHostTable(query, host);
        }

        public bool DeleteByHostKeyPrefixAndDate(DateTime date, string host, string keyPrefix)
        {
            string query =
                "DELETE FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = CAST(@Date AS date)";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Host", host },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        public bool Insert(DateTime date, string host, string key, int counter)
        {
            string query =
                "DELETE FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] = @Key " +
                "AND CAST([Date] AS date) = CAST(@Date AS date); " +
                "INSERT INTO " + HostStatistics.HOST_STATISTICS + " ([Date], [Host], [Key], [Counter]) " +
                "VALUES (@Date, @Host, @Key, @Counter);";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Host", host },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public bool InsertDailyCounter(string host, string key, int counter)
        {
            string query =
                "MERGE " + HostStatistics.HOST_STATISTICS + " AS target " +
                "USING (" +
                "   SELECT " +
                "       CAST(@Date AS DATE) AS DateOnly, " +
                "       @Host AS [Host], " +
                "       @Key AS [Key], " +
                "       @Counter AS [Counter] " +
                "   ) AS source " +
                "ON " +
                "   CAST(target.[Date] AS DATE) = source.DateOnly " +
                "   AND target.[Host] = source.[Host] " +
                "   AND target.[Key] = source.[Key] " +
                "WHEN MATCHED THEN " +
                "   UPDATE SET target.[Counter] = target.[Counter] + source.[Counter] " +
                "WHEN NOT MATCHED THEN " +
                "   INSERT ([Date], [Host], [Key], [Counter]) " +
                "   VALUES (source.DateOnly, source.[Host], source.[Key], source.[Counter]);";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Date", DateTime.Now },
                { "Host", host },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public DataTable GetLatestStatistics(string host, string statisticsKey, int top)
        {
            string query =
                "SELECT TOP (@Top) [Date], [Counter] " +
                "FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host AND [Key] = @Key " +
                "ORDER BY [Date] DESC";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Top", top },
                { "Host", host },
                { "Key", statisticsKey }
            });
        }

        public DataTable GetLatestStatisticsByPrefix(string host, string keyPrefix)
        {
            string query =
                "SELECT [Key], [Counter] " +
                "FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = (" +
                "   SELECT MAX(CAST([Date] AS date)) " +
                "   FROM " + HostStatistics.HOST_STATISTICS + " " +
                "   WHERE [Host] = @Host AND [Key] LIKE @KeyPrefix" +
                ") " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        public DataTable GetPagesByPageType(string host)
        {
            string query =
                "SELECT CONVERT(NVARCHAR(100), [PageType]) AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "AND [PageType] IS NOT NULL " +
                "GROUP BY [PageType] " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return QueryHostTable(query, host);
        }

        public DataTable GetPagesByHttpStatusCode(string host)
        {
            string query =
                "SELECT COALESCE(CONVERT(NVARCHAR(10), [HttpStatusCode]), 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "GROUP BY [HttpStatusCode] " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return QueryHostTable(query, host);
        }

        public DataTable GetPagesByNextScrape(string host)
        {
            string query =
                "SELECT COALESCE(CONVERT(VARCHAR, [NextScrape], 23), 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "GROUP BY CONVERT(VARCHAR, [NextScrape], 23) " +
                "ORDER BY [Key] ASC";

            return QueryHostTable(query, host);
        }

        public DataTable GetPublishedListingsByOperation(string host)
        {
            string query =
                "SELECT COALESCE([operation], 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host " +
                "AND [listingStatus] = @ListingStatus " +
                "GROUP BY [operation] " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return QueryPublishedListingsDistribution(host, query);
        }

        public DataTable GetPublishedListingsByPropertyType(string host)
        {
            string query =
                "SELECT COALESCE([propertyType], 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host " +
                "AND [listingStatus] = @ListingStatus " +
                "GROUP BY [propertyType] " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return QueryPublishedListingsDistribution(host, query);
        }

        public DataTable GetListingsByLocationResolver(string host)
        {
            string query =
                "SELECT COALESCE(NULLIF(LTRIM(RTRIM([locationResolver])), ''), 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host " +
                "GROUP BY COALESCE(NULLIF(LTRIM(RTRIM([locationResolver])), ''), 'NULL') " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return QueryHostTable(query, host);
        }

        public DataTable GetUnpublishedListingsByUnlistingReason(string host)
        {
            string query =
                "SELECT COALESCE([unlistingReason], 'NULL') AS [Key], COUNT(*) AS [Counter] " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host " +
                "AND [listingStatus] = @ListingStatus " +
                "GROUP BY [unlistingReason] " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", ListingStatus.unpublished.ToString() }
            });
        }

        public List<string> GetKeysLike(string host, HostStatisticsKey key)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @Key " +
                "ORDER BY [Key] ASC";

            return Database.QueryListString(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "Key", key + "_%" }
            });
        }

        public DateTime? GetLatestDate(string host)
        {
            string query =
                "SELECT MAX([Date]) " +
                "FROM " + HostStatistics.HOST_STATISTICS + " " +
                "WHERE [Host] = @Host";

            var value = Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            }).Rows[0][0];

            return value is DBNull ? null : (DateTime)value;
        }

        private int QueryHostInt(string query, string host)
        {
            return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host }
            });
        }

        private DataTable QueryHostTable(string query, string host)
        {
            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            });
        }

        private DataTable QueryPublishedListingsDistribution(string host, string query)
        {
            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", ListingStatus.published.ToString() }
            });
        }
    }
}
