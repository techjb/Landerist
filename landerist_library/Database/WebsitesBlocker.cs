using landerist_library.Websites;

namespace landerist_library.Database
{
    public class WebsitesBlocker
    {
        public const string WEBSITES_BLOCKER = "[WEBSITES_BLOCKER]";

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
            var hostBlockUntil = CalculateForbiddenBlockUntil(transientErrorCounter);
            return Block(website, hostBlockUntil);
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
               "   SET BlockUntil = @HostBlockUntil " +
               "   WHERE IpOrHost = @Host " +
               "END " +
               "ELSE BEGIN " +
               "   INSERT INTO " + WEBSITES_BLOCKER + " (IpOrHost, BlockUntil) " +
               "   VALUES (@Host, @HostBlockUntil) " +
               "END";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"HostBlockUntil", hostBlockUntil},
            });
        }

        public static bool Clean()
        {
            string query = "DELETE FROM [WEBSITES_BLOCKER] WHERE BlockUntil < GETDATE()";
            return new DataBase().Query(query);
        }

        private static DateTime CalculateHostBlockUntil(Website website)
        {
            int randomMilliseconds = Random.Shared.Next(3000, 6000);
            int crawlDelayMilliseconds = Math.Min(website.CrawlDelay(), Configuration.Config.MAX_CRAW_DELAY_SECONDS) * 1000;
            int milliseconds = Math.Max(randomMilliseconds, crawlDelayMilliseconds);
            return DateTime.Now.AddMilliseconds(milliseconds);
        }

        private static DateTime CalculateForbiddenBlockUntil(short? transientErrorCounter)
        {
            int attempts = Math.Max(1, (int)(transientErrorCounter ?? 1));
            int blockMinutes = attempts switch
            {
                1 => 2,
                2 => 5,
                3 => 15,
                4 => 30,
                5 => 60,
                _ => 180,
            };

            int jitterSeconds = Random.Shared.Next(0, Math.Max(30, blockMinutes * 30));
            return DateTime.Now.AddMinutes(blockMinutes).AddSeconds(jitterSeconds);
        }
    }
}
