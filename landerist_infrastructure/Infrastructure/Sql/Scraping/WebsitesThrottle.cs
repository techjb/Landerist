using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Websites;
using System.Text;

namespace landerist_library.Infrastructure.Sql.Scraping
{
    public sealed class WebsitesThrottle
    {
        public const string WEBSITES_THROTTLE = "[WEBSITES_THROTTLE]";
        private readonly IDatabase _database;
        private readonly IWebsiteRobotsPolicy _robots;

        private static readonly int[] ForbiddenRetryDelaySecondsByLevel =
        [
            0, 30, 60, 90, 120, 180, 240, 300, 450, 600, 900, 1200, 1800, 2700, 3600, 5400, 7200, 10800, 14400, 21600, 28800, 43200, 86400
        ];

        private static readonly short MAX_FORBIDDEN_BACKOFF_LEVEL = (short)(ForbiddenRetryDelaySecondsByLevel.Length - 1);
        private const int SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF = 3;
        private const int MIN_SECONDS_BETWEEN_FORBIDDEN_BACKOFF_DECREASES = 300;
        private const int MAX_FORBIDDEN_JITTER_SECONDS = 300;
        private const double FORBIDDEN_JITTER_RATIO = 0.2d;

        public WebsitesThrottle(IDatabase database, IWebsiteRobotsPolicy robots)
        {
            ArgumentNullException.ThrowIfNull(database);
            ArgumentNullException.ThrowIfNull(robots);
            _database = database;
            _robots = robots;
        }

        public bool IsBlocked(Website website) =>
            _database.QueryBool(GetIsBlockedQuery(), GetHostParameters(website));

        public Task<bool> IsBlockedAsync(
            Website website,
            CancellationToken cancellationToken = default) =>
            _database.QueryBoolAsync(
                GetIsBlockedQuery(),
                GetHostParameters(website),
                cancellationToken);

        public bool Block(Website website)
        {
            int delayMilliseconds = CalculateHostBlockDelayMilliseconds(website);
            return Block(website, delayMilliseconds);
        }

        public Task<bool> BlockAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            int delayMilliseconds = CalculateHostBlockDelayMilliseconds(website);
            (string query, Dictionary<string, object?> parameters) =
                GetBlockCommand(website, delayMilliseconds);
            return _database.QueryBoolAsync(query, parameters, cancellationToken);
        }
        public bool ReportForbidden(Website website)
        {
            (string query, Dictionary<string, object?> parameters) =
                GetReportForbiddenCommand(website);
            return _database.Query(query, parameters);
        }

        public Task<bool> ReportForbiddenAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            (string query, Dictionary<string, object?> parameters) =
                GetReportForbiddenCommand(website);
            return _database.QueryAsync(query, parameters, cancellationToken);
        }

        private static (string Query, Dictionary<string, object?> Parameters)
            GetReportForbiddenCommand(Website website)
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
                "WHERE Host = @Host; " +
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
                "WHERE Host = @Host; " +
                "IF @@ROWCOUNT = 0 " +
                "BEGIN " +
                "   INSERT INTO " + WEBSITES_THROTTLE + " " +
                "       (Host, BlockUntil, ForbiddenBackoffLevel, ForbiddenRetryDelaySeconds, ForbiddenCounter, SuccessCounterAfterForbidden, LastForbiddenAt, Updated) " +
                "   VALUES " +
                "       (@Host, @HostBlockUntil, @NewForbiddenBackoffLevel, @ForbiddenRetryDelaySeconds, 1, 0, @Now, @Now); " +
                "END; " +
                "COMMIT TRANSACTION";

            return (query, new Dictionary<string, object?>
            {
                {"Host", website.Host},
                {"MaxForbiddenBackoffLevel", MAX_FORBIDDEN_BACKOFF_LEVEL},
                {"ForbiddenJitterRatio", FORBIDDEN_JITTER_RATIO},
                {"MaxForbiddenJitterSeconds", MAX_FORBIDDEN_JITTER_SECONDS},
            });
        }

        public bool ReportSuccess(Website website)
        {
            (string query, Dictionary<string, object?> parameters) =
                GetReportSuccessCommand(website);
            return _database.Query(query, parameters);
        }

        public Task<bool> ReportSuccessAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            (string query, Dictionary<string, object?> parameters) =
                GetReportSuccessCommand(website);
            return _database.QueryAsync(query, parameters, cancellationToken);
        }

        private static (string Query, Dictionary<string, object?> Parameters)
            GetReportSuccessCommand(Website website)
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
                "WHERE Host = @Host; " +
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
                "   WHERE Host = @Host; " +
                "END; " +
                "COMMIT TRANSACTION";

            return (query, new Dictionary<string, object?>
            {
                {"Host", website.Host},
                {"SuccessesToDecreaseForbiddenBackoff", SUCCESSES_TO_DECREASE_FORBIDDEN_BACKOFF},
                {"MinSecondsBetweenForbiddenBackoffDecreases", MIN_SECONDS_BETWEEN_FORBIDDEN_BACKOFF_DECREASES},
            });
        }

        private bool Block(Website website, int hostBlockDelayMilliseconds)
        {
            (string query, Dictionary<string, object?> parameters) =
                GetBlockCommand(website, hostBlockDelayMilliseconds);
            return _database.QueryBool(query, parameters);
        }

        private static (string Query, Dictionary<string, object?> Parameters) GetBlockCommand(
            Website website,
            int hostBlockDelayMilliseconds)
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
                "WHERE Host = @Host " +
                "AND BlockUntil <= @Now; " +
                "IF @@ROWCOUNT > 0 " +
                "BEGIN " +
                "   SET @Acquired = 1; " +
                "END " +
                "ELSE IF NOT EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + WEBSITES_THROTTLE + " WITH (UPDLOCK, HOLDLOCK) " +
                "   WHERE Host = @Host" +
                ") " +
                "BEGIN " +
                "   INSERT INTO " + WEBSITES_THROTTLE + " (Host, BlockUntil, Updated) " +
                "   VALUES (@Host, @HostBlockUntil, @Now); " +
                "   SET @Acquired = 1; " +
                "END; " +
                "COMMIT TRANSACTION; " +
                "SELECT @Acquired";

            return (query, new Dictionary<string, object?>
            {
                {"Host", website.Host},
                {"HostBlockDelayMilliseconds", hostBlockDelayMilliseconds},
            });
        }

        private static string GetIsBlockedQuery() =>
            "SELECT " +
            "   CAST(" +
            "      CASE " +
            "           WHEN MAX(BlockUntil) > GETDATE() THEN 1 " +
            "           ELSE 0 " +
            "       END " +
            "   AS BIT) AS IsBlocked " +
            "FROM " + WEBSITES_THROTTLE + " " +
            "WHERE Host = @Host";

        private static Dictionary<string, object?> GetHostParameters(Website website) =>
            new() { ["Host"] = website.Host };
        public bool Clean() => _database.Query(GetCleanQuery());

        public Task<bool> CleanAsync(CancellationToken cancellationToken = default) =>
            _database.QueryAsync(
                GetCleanQuery(),
                cancellationToken: cancellationToken);

        private static string GetCleanQuery() =>
            "DELETE FROM " + WEBSITES_THROTTLE + " " +
            "WHERE BlockUntil < GETDATE() " +
            "AND ISNULL(ForbiddenBackoffLevel, 0) = 0 " +
            "AND ISNULL(ForbiddenCounter, 0) = 0";

        private int CalculateHostBlockDelayMilliseconds(Website website)
        {
            int randomMilliseconds = Random.Shared.Next(3000, 6000);
            int crawlDelayMilliseconds = Math.Min(_robots.GetCrawlDelaySeconds(website), website.Rules.MaxCrawlDelaySeconds) * 1000;
            int configuredMinimumMilliseconds = Math.Max(0, website.MinimumRequestIntervalMilliseconds ?? 0);
            return Math.Max(Math.Max(randomMilliseconds, crawlDelayMilliseconds), configuredMinimumMilliseconds);
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
