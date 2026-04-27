using landerist_library.Websites;

namespace landerist_library.Database
{
    public class WebsitesBlocker
    {
        public const string WEBSITES_BLOCKER = "[WEBSITES_BLOCKER]";
        private const short MAX_FORBIDDEN_BACKOFF_LEVEL = 17;
        private const int SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF = 3;

        public static bool IsBlocked(Website website)
        {
            string query =
                "SELECT " +
                "   CAST(" +
                "      CASE " +
                "           WHEN MAX(BlockUntil) > GETDATE() THEN 1 " +
                "           ELSE 0 " +
                "       END " +
                "   AS BIT) AS IsBlocked " +
                "FROM " + WEBSITES_BLOCKER + " " +
                "WHERE IpOrHost = @Host";

            return new DataBase().QueryBool(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host}
            });
        }

        public static bool Block(Website website)
        {
            var hostBlockUntil = CalculateHostBlockUntil(website);
            return Block(website, hostBlockUntil);
        }

        public static bool BlockForbidden(Website website, short? transientErrorCounter = null)
        {
            return ReportForbidden(website);
        }

        public static bool ReportForbidden(Website website)
        {
            int jitterSeconds = Random.Shared.Next(0, 90);
            string query =
                "DECLARE @Now datetime = GETDATE(); " +
                "DECLARE @NewForbiddenBackoffLevel smallint; " +
                "DECLARE @ForbiddenRetryDelaySeconds int; " +
                "DECLARE @HostBlockUntil datetime; " +
                "IF EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + WEBSITES_BLOCKER + " WITH (UPDLOCK, HOLDLOCK) " +
                "   WHERE IpOrHost = @Host) " +
                "BEGIN " +
                "   SELECT @NewForbiddenBackoffLevel = " +
                "       CASE " +
                "           WHEN ISNULL(ForbiddenBackoffLevel, 0) >= @MaxForbiddenBackoffLevel THEN @MaxForbiddenBackoffLevel " +
                "           ELSE ISNULL(ForbiddenBackoffLevel, 0) + 1 " +
                "       END " +
                "   FROM " + WEBSITES_BLOCKER + " " +
                "   WHERE IpOrHost = @Host; " +
                "END " +
                "ELSE BEGIN " +
                "   SET @NewForbiddenBackoffLevel = 1; " +
                "END " +
                "SET @ForbiddenRetryDelaySeconds = " + GetForbiddenDelaySecondsSql("@NewForbiddenBackoffLevel") + "; " +
                "SET @HostBlockUntil = DATEADD(second, @ForbiddenRetryDelaySeconds + @JitterSeconds, @Now); " +
                "UPDATE " + WEBSITES_BLOCKER + " " +
                "SET " +
                "   BlockUntil = CASE WHEN BlockUntil > @HostBlockUntil THEN BlockUntil ELSE @HostBlockUntil END, " +
                "   ForbiddenBackoffLevel = @NewForbiddenBackoffLevel, " +
                "   ForbiddenRetryDelaySeconds = @ForbiddenRetryDelaySeconds, " +
                "   ForbiddenCounter = ISNULL(ForbiddenCounter, 0) + 1, " +
                "   SuccessCounterAfterForbidden = 0, " +
                "   LastForbiddenAt = @Now, " +
                "   Updated = @Now " +
                "WHERE IpOrHost = @Host; " +
                "IF @@ROWCOUNT = 0 " +
                "BEGIN " +
                "   INSERT INTO " + WEBSITES_BLOCKER + " " +
                "       (IpOrHost, BlockUntil, ForbiddenBackoffLevel, ForbiddenRetryDelaySeconds, ForbiddenCounter, SuccessCounterAfterForbidden, LastForbiddenAt, Updated) " +
                "   VALUES " +
                "       (@Host, @HostBlockUntil, @NewForbiddenBackoffLevel, @ForbiddenRetryDelaySeconds, 1, 0, @Now, @Now); " +
                "END";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"MaxForbiddenBackoffLevel", MAX_FORBIDDEN_BACKOFF_LEVEL},
                {"JitterSeconds", jitterSeconds},
            });
        }

        public static bool ReportSuccess(Website website)
        {
            string query =
                "DECLARE @Now datetime = GETDATE(); " +
                "DECLARE @CurrentForbiddenBackoffLevel smallint; " +
                "DECLARE @NewForbiddenBackoffLevel smallint; " +
                "DECLARE @NewSuccessCounterAfterForbidden int; " +
                "SELECT " +
                "   @CurrentForbiddenBackoffLevel = ISNULL(ForbiddenBackoffLevel, 0), " +
                "   @NewSuccessCounterAfterForbidden = ISNULL(SuccessCounterAfterForbidden, 0) + 1 " +
                "FROM " + WEBSITES_BLOCKER + " WITH (UPDLOCK, HOLDLOCK) " +
                "WHERE IpOrHost = @Host; " +
                "IF @CurrentForbiddenBackoffLevel IS NOT NULL AND @CurrentForbiddenBackoffLevel > 0 " +
                "BEGIN " +
                "   SET @NewForbiddenBackoffLevel = " +
                "       CASE " +
                "           WHEN @NewSuccessCounterAfterForbidden >= @SuccessesToDecreaseForbiddenBackoff THEN @CurrentForbiddenBackoffLevel - 1 " +
                "           ELSE @CurrentForbiddenBackoffLevel " +
                "       END; " +
                "   UPDATE " + WEBSITES_BLOCKER + " " +
                "   SET " +
                "       ForbiddenBackoffLevel = @NewForbiddenBackoffLevel, " +
                "       ForbiddenRetryDelaySeconds = " + GetForbiddenDelaySecondsSql("@NewForbiddenBackoffLevel") + ", " +
                "       SuccessCounterAfterForbidden = " +
                "           CASE " +
                "               WHEN @NewSuccessCounterAfterForbidden >= @SuccessesToDecreaseForbiddenBackoff THEN 0 " +
                "               ELSE @NewSuccessCounterAfterForbidden " +
                "           END, " +
                "       LastSuccessAt = @Now, " +
                "       Updated = @Now " +
                "   WHERE IpOrHost = @Host; " +
                "END";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"SuccessesToDecreaseForbiddenBackoff", SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF},
            });
        }

        private static bool Block(Website website, DateTime hostBlockUntil)
        {
            string query =
                "IF EXISTS (" +
               "   SELECT 1 " +
               "   FROM " + WEBSITES_BLOCKER + " " +
               "   WHERE IpOrHost = @Host) " +
               "BEGIN " +
               "   UPDATE " + WEBSITES_BLOCKER + " " +
               "   SET " +
               "       BlockUntil = CASE WHEN BlockUntil > @HostBlockUntil THEN BlockUntil ELSE @HostBlockUntil END, " +
               "       Updated = GETDATE() " +
               "   WHERE IpOrHost = @Host " +
               "END " +
               "ELSE BEGIN " +
               "   INSERT INTO " + WEBSITES_BLOCKER + " (IpOrHost, BlockUntil, Updated) " +
               "   VALUES (@Host, @HostBlockUntil, GETDATE()) " +
               "END";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"HostBlockUntil", hostBlockUntil},
            });
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + WEBSITES_BLOCKER + " " +
                "WHERE BlockUntil < GETDATE() " +
                "AND ISNULL(ForbiddenBackoffLevel, 0) = 0 " +
                "AND ISNULL(ForbiddenCounter, 0) = 0";
            return new DataBase().Query(query);
        }

        private static DateTime CalculateHostBlockUntil(Website website)
        {
            int randomMilliseconds = Random.Shared.Next(3000, 6000);
            int crawlDelayMilliseconds = Math.Min(website.CrawlDelay(), Configuration.Config.MAX_CRAW_DELAY_SECONDS) * 1000;
            int milliseconds = Math.Max(randomMilliseconds, crawlDelayMilliseconds);
            return DateTime.Now.AddMilliseconds(milliseconds);
        }

        private static string GetForbiddenDelaySecondsSql(string forbiddenBackoffLevelExpression)
        {
            return
                "CASE " +
                "   WHEN " + forbiddenBackoffLevelExpression + " <= 0 THEN 0 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 1 THEN 30 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 2 THEN 60 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 3 THEN 90 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 4 THEN 120 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 5 THEN 180 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 6 THEN 240 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 7 THEN 300 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 8 THEN 450 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 9 THEN 600 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 10 THEN 900 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 11 THEN 1200 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 12 THEN 1800 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 13 THEN 2700 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 14 THEN 3600 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 15 THEN 5400 " +
                "   WHEN " + forbiddenBackoffLevelExpression + " = 16 THEN 7200 " +
                "   ELSE 10800 " +
                "END";
        }
    }
}
