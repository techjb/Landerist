using landerist_library.Websites;
using System.Text;

namespace landerist_library.Database
{
    public class WebsitesThrottle
    {
        public const string WEBSITES_THROTTLE = "[WEBSITES_THROTTLE]";

        private static readonly int[] ForbiddenRetryDelaySecondsByLevel =
        [
            0, 30, 60, 90, 120, 180, 240, 300, 450, 600, 900, 1200, 1800, 2700, 3600, 5400, 7200, 10800, 14400, 21600, 28800, 43200, 86400
        ];

        private static readonly short MAX_FORBIDDEN_BACKOFF_LEVEL = (short)(ForbiddenRetryDelaySecondsByLevel.Length - 1);
        private const int SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF = 3;
        private const int MIN_SECONDS_BETWEEN_FORBIDDEN_BACKOFF_DECREASES = 300;
        private const int MAX_FORBIDDEN_JITTER_SECONDS = 300;
        private const double FORBIDDEN_JITTER_RATIO = 0.2d;

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
                "FROM " + WEBSITES_THROTTLE + " " +
                "WHERE IpOrHost = @Host";

            return new DataBase().QueryBool(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host}
            });
        }

        public static bool Block(Website website)
        {
            var hostBlockDelayMilliseconds = CalculateHostBlockDelayMilliseconds(website);
            return Block(website, hostBlockDelayMilliseconds);
        }

        public static bool ReportForbidden(Website website)
        {
            string query =
                "SET XACT_ABORT ON; " +
                "BEGIN TRANSACTION; " +
                "DECLARE @Now datetime = GETDATE(); " +
                "DECLARE @NewForbiddenBackoffLevel smallint; " +
                "DECLARE @ForbiddenRetryDelaySeconds int; " +
                "DECLARE @MaxJitterSeconds int; " +
                "DECLARE @JitterSeconds int; " +
                "DECLARE @HostBlockUntil datetime; " +
                "SELECT @NewForbiddenBackoffLevel = " +
                "       CASE " +
                "           WHEN ISNULL(ForbiddenBackoffLevel, 0) >= @MaxForbiddenBackoffLevel THEN @MaxForbiddenBackoffLevel " +
                "           ELSE ISNULL(ForbiddenBackoffLevel, 0) + 1 " +
                "       END " +
                "FROM " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
                "WHERE IpOrHost = @Host; " +
                "IF @NewForbiddenBackoffLevel IS NULL " +
                "BEGIN " +
                "   SET @NewForbiddenBackoffLevel = 1; " +
                "END; " +
                "SET @ForbiddenRetryDelaySeconds = " + GetForbiddenDelaySecondsSql("@NewForbiddenBackoffLevel") + "; " +
                "SET @MaxJitterSeconds = " +
                "   CASE " +
                "       WHEN @ForbiddenRetryDelaySeconds <= 0 THEN 0 " +
                "       WHEN CAST(@ForbiddenRetryDelaySeconds * @ForbiddenJitterRatio AS int) > @MaxForbiddenJitterSeconds THEN @MaxForbiddenJitterSeconds " +
                "       ELSE CAST(@ForbiddenRetryDelaySeconds * @ForbiddenJitterRatio AS int) " +
                "   END; " +
                "SET @JitterSeconds = " +
                "   CASE " +
                "       WHEN @MaxJitterSeconds <= 0 THEN 0 " +
                "       ELSE ABS(CHECKSUM(NEWID()) % (@MaxJitterSeconds + 1)) " +
                "   END; " +
                "SET @HostBlockUntil = DATEADD(second, @ForbiddenRetryDelaySeconds + @JitterSeconds, @Now); " +
                "UPDATE " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
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
                "   INSERT INTO " + WEBSITES_THROTTLE + " " +
                "       (IpOrHost, BlockUntil, ForbiddenBackoffLevel, ForbiddenRetryDelaySeconds, ForbiddenCounter, SuccessCounterAfterForbidden, LastForbiddenAt, Updated) " +
                "   VALUES " +
                "       (@Host, @HostBlockUntil, @NewForbiddenBackoffLevel, @ForbiddenRetryDelaySeconds, 1, 0, @Now, @Now); " +
                "END; " +
                "COMMIT TRANSACTION";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"MaxForbiddenBackoffLevel", MAX_FORBIDDEN_BACKOFF_LEVEL},
                {"ForbiddenJitterRatio", FORBIDDEN_JITTER_RATIO},
                {"MaxForbiddenJitterSeconds", MAX_FORBIDDEN_JITTER_SECONDS},
            });
        }

        public static bool ReportSuccess(Website website)
        {
            string query =
                "SET XACT_ABORT ON; " +
                "BEGIN TRANSACTION; " +
                "DECLARE @Now datetime = GETDATE(); " +
                "DECLARE @CurrentForbiddenBackoffLevel smallint; " +
                "DECLARE @NewForbiddenBackoffLevel smallint; " +
                "DECLARE @NewSuccessCounterAfterForbidden int; " +
                "DECLARE @LastBackoffEventAt datetime; " +
                "SELECT " +
                "   @CurrentForbiddenBackoffLevel = ISNULL(ForbiddenBackoffLevel, 0), " +
                "   @NewSuccessCounterAfterForbidden = ISNULL(SuccessCounterAfterForbidden, 0) + 1, " +
                "   @LastBackoffEventAt = COALESCE(LastSuccessAt, LastForbiddenAt) " +
                "FROM " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
                "WHERE IpOrHost = @Host; " +
                "IF @CurrentForbiddenBackoffLevel IS NOT NULL AND @CurrentForbiddenBackoffLevel > 0 " +
                "BEGIN " +
                "   SET @NewForbiddenBackoffLevel = " +
                "       CASE " +
                "           WHEN @NewSuccessCounterAfterForbidden >= @SuccessesToDecreaseForbiddenBackoff " +
                "               AND (" +
                "                   @LastBackoffEventAt IS NULL " +
                "                   OR DATEDIFF(second, @LastBackoffEventAt, @Now) >= @MinSecondsBetweenForbiddenBackoffDecreases " +
                "               ) THEN @CurrentForbiddenBackoffLevel - 1 " +
                "           ELSE @CurrentForbiddenBackoffLevel " +
                "       END; " +
                "   UPDATE " + WEBSITES_THROTTLE + " " +
                "   SET " +
                "       ForbiddenBackoffLevel = @NewForbiddenBackoffLevel, " +
                "       ForbiddenRetryDelaySeconds = " + GetForbiddenDelaySecondsSql("@NewForbiddenBackoffLevel") + ", " +
                "       SuccessCounterAfterForbidden = " +
                "           CASE " +
                "               WHEN @NewForbiddenBackoffLevel < @CurrentForbiddenBackoffLevel THEN 0 " +
                "               ELSE @NewSuccessCounterAfterForbidden " +
                "           END, " +
                "       LastSuccessAt = CASE WHEN @NewForbiddenBackoffLevel < @CurrentForbiddenBackoffLevel THEN @Now ELSE LastSuccessAt END, " +
                "       Updated = @Now " +
                "   WHERE IpOrHost = @Host; " +
                "END; " +
                "COMMIT TRANSACTION";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"SuccessesToDecreaseForbiddenBackoff", SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF},
                {"MinSecondsBetweenForbiddenBackoffDecreases", MIN_SECONDS_BETWEEN_FORBIDDEN_BACKOFF_DECREASES},
            });
        }

        private static bool Block(Website website, int hostBlockDelayMilliseconds)
        {
            string query =
                "SET XACT_ABORT ON; " +
                "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
                "BEGIN TRANSACTION; " +
                "DECLARE @Now datetime = GETDATE(); " +
                "DECLARE @Acquired bit = 0; " +
                "DECLARE @HostBlockUntil datetime = DATEADD(millisecond, @HostBlockDelayMilliseconds, @Now); " +
                "UPDATE " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
                "SET " +
                "   BlockUntil = CASE WHEN BlockUntil > @HostBlockUntil THEN BlockUntil ELSE @HostBlockUntil END, " +
                "   Updated = @Now " +
                "WHERE IpOrHost = @Host " +
                "AND BlockUntil <= @Now; " +
                "IF @@ROWCOUNT > 0 " +
                "BEGIN " +
                "   SET @Acquired = 1; " +
                "END " +
                "ELSE IF NOT EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
                "   WHERE IpOrHost = @Host" +
                ") " +
                "BEGIN " +
                "   INSERT INTO " + WEBSITES_THROTTLE + " (IpOrHost, BlockUntil, Updated) " +
                "   VALUES (@Host, @HostBlockUntil, @Now); " +
                "   SET @Acquired = 1; " +
                "END; " +
                "COMMIT TRANSACTION; " +
                "SELECT @Acquired";

            return new DataBase().QueryBool(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"HostBlockDelayMilliseconds", hostBlockDelayMilliseconds},
            });
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + WEBSITES_THROTTLE + " " +
                "WHERE BlockUntil < GETDATE() " +
                "AND ISNULL(ForbiddenBackoffLevel, 0) = 0 " +
                "AND ISNULL(ForbiddenCounter, 0) = 0";
            return new DataBase().Query(query);
        }

        private static int CalculateHostBlockDelayMilliseconds(Website website)
        {
            int randomMilliseconds = Random.Shared.Next(3000, 6000);
            int crawlDelayMilliseconds = Math.Min(website.CrawlDelay(), Configuration.Config.MAX_CRAW_DELAY_SECONDS) * 1000;
            return Math.Max(randomMilliseconds, crawlDelayMilliseconds);
        }

        private static string GetForbiddenDelaySecondsSql(string forbiddenBackoffLevelExpression)
        {
            var caseExpression = new StringBuilder("CASE ");
            for (int level = 0; level < ForbiddenRetryDelaySecondsByLevel.Length - 1; level++)
            {
                caseExpression
                    .Append("WHEN ")
                    .Append(forbiddenBackoffLevelExpression)
                    .Append(" = ")
                    .Append(level)
                    .Append(" THEN ")
                    .Append(ForbiddenRetryDelaySecondsByLevel[level])
                    .Append(' ');
            }

            caseExpression
                .Append("ELSE ")
                .Append(ForbiddenRetryDelaySecondsByLevel[^1])
                .Append(" END");

            return caseExpression.ToString();
        }
    }
}
