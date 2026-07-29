using landerist_library.Infrastructure.Sql;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Application.Statistics;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Statistics
{
    public sealed class GlobalStatisticsRepository : IGlobalStatisticsRepository
    {
        private const string GlobalStatisticsTable = "[GLOBAL_STATISTICS]";
        private readonly IDatabase _database;
        public GlobalStatisticsRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database;

        public int CountWebsites()
        {
            return QueryInt("SELECT COUNT(*) FROM " + Websites.Websites.WEBSITES);
        }

        public int CountUpdatedRobotsTxtYesterday()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [RobotsTxtUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryInt(query);
        }

        public int CountUpdatedSitemapsYesterday()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [SitemapUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryInt(query);
        }

        public int CountUpdatedIpAddressYesterday()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [IpAddressUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryInt(query);
        }

        public int CountPages()
        {
            return QueryInt("SELECT COUNT(*) FROM " + Pages.Pages.PAGES);
        }

        public int CountLastScrapePagesYesterday()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE CONVERT(date, [LastScrape]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            return QueryInt(query);
        }

        public int CountNeedUpdatePages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [NextScrape] < GETDATE()";

            return QueryInt(query);
        }

        public int CountWaitingAIRequestPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [WaitingStatus] = @WaitingStatus";

            return QueryInt(query, new Dictionary<string, object?>
            {
                { "WaitingStatus", WaitingStatus.waiting_ai_request.ToString() }
            });
        }

        public int CountUnknownPageTypePages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [PageType] IS NULL";

            return QueryInt(query);
        }

        public int CountListings()
        {
            return QueryInt("SELECT COUNT(*) FROM " + SqlTableNames.Listings);
        }

        public int CountListings(ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + SqlTableNames.Listings + " " +
                "WHERE [listingStatus] = @ListingStatus";

            return QueryInt(query, new Dictionary<string, object?>
            {
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        public int CountMedia()
        {
            return QueryInt("SELECT COUNT(*) FROM " + SqlTableNames.Media);
        }

        public DataTable GetHttpStatusCodeCounts(DateTime date)
        {
            string query =
                "SELECT [HttpStatusCode], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE CAST([LastScrape] AS date) = CAST(@Date AS date) " +
                "GROUP BY [HttpStatusCode] ";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Date", date }
            });
        }

        public DataTable GetPageTypeCounts(DateTime date)
        {
            string query =
                "SELECT [PageType], COUNT(*) AS [Counter] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE CAST([LastScrape] AS date) = CAST(@Date AS date) " +
                "AND [PageType] IS NOT NULL " +
                "GROUP BY [PageType] ";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Date", date }
            });
        }

        public List<string> GetKeysLike(StatisticsKey key)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + GlobalStatisticsTable + " " +
                "WHERE [Key] LIKE @Key";

            return Database.QueryListString(query, new Dictionary<string, object?>
            {
                { "Key", key + "%" }
            });
        }

        public bool DeleteByKeyPrefixAndDate(DateTime date, string keyPrefix)
        {
            string query =
                "DELETE FROM " + GlobalStatisticsTable + " " +
                "WHERE [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = CAST(@Date AS date)";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        public bool Insert(DateTime date, string key, int counter)
        {
            string query =
                "DELETE FROM " + GlobalStatisticsTable + " " +
                "WHERE [Key] = @Key AND CAST([Date] AS date) = CAST(@Date AS date); " +
                "INSERT INTO " + GlobalStatisticsTable + " ([Date], [Key], [Counter]) " +
                "VALUES (@Date, @Key, @Counter);";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public bool InsertDailyCounter(string key, int counter) =>
            InsertDailyCounterCore(key, counter, Database.Query);

        public Task<bool> InsertDailyCounterAsync(string key, int counter, CancellationToken cancellationToken = default) =>
            InsertDailyCounterCore(key, counter, (query, parameters) => Database.QueryAsync(query, parameters, cancellationToken));

        private TResult InsertDailyCounterCore<TResult>(string key, int counter, Func<string, IDictionary<string, object?>, TResult> execute)
        {
            string query =
                "MERGE " + GlobalStatisticsTable + " AS target " +
                "USING (" +
                "   SELECT " +
                "       CAST(@Date AS DATE) AS DateOnly, " +
                "       @Key AS [Key], " +
                "       @Counter AS [Counter] " +
                "   ) AS source " +
                "ON " +
                "   CAST(target.[Date] AS DATE) = source.DateOnly " +
                "   AND target.[Key] = source.[Key] " +
                "WHEN MATCHED THEN " +
                "   UPDATE SET target.[Counter] = target.[Counter] + source.[Counter] " +
                "WHEN NOT MATCHED THEN " +
                "   INSERT ([Date], [Key], [Counter]) " +
                "   VALUES (source.DateOnly, source.[Key], source.[Counter]);";

            return execute(query, new Dictionary<string, object?>
            {
                { "Date", DateTime.Now },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public DataTable GetStatisticsKeys()
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + GlobalStatisticsTable + " ";

            return Database.QueryTable(query);
        }

        public DataTable GetStatistics(string key, int months)
        {
            string query =
                "SELECT [Date], [Key], [Counter] " +
                "FROM " + GlobalStatisticsTable + " " +
                "WHERE [Key] = @Key AND " +
                "[Date] > DATEADD(MONTH, @Months, GETDATE()) " +
                "ORDER BY [Date] ASC";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Key", key },
                { "Months", months }
            });
        }

        public DataTable GetLatestStatistics(string statisticsKey, int top)
        {
            string query =
                "SELECT TOP (@Top) [Date], [Counter] " +
                "FROM " + GlobalStatisticsTable + " " +
                "WHERE [Key] = @Key " +
                "ORDER BY [Date] DESC";

            return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Top", top },
                { "Key", statisticsKey }
            });
        }

        private int QueryInt(string query)
        {
            return Database.QueryInt(query);
        }

        private int QueryInt(string query, Dictionary<string, object?> parameters)
        {
            DataTable dataTable = Database.QueryTable(query, parameters);
            return Convert.ToInt32(dataTable.Rows[0][0]);
        }
    }
}
