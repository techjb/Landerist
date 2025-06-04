using landerist_library.Websites;

namespace landerist_library.Database
{
    public class WebsitesBlocker
    {
        private const int MinSecconds = 3;

        private const int MaxSecconds = 6;

        private const string WEBSITES_BLOCKER = "[WEBSITES_BLOCKER]";

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
                "FROM " + WEBSITES_BLOCKER +
                "WHERE IpOrHost IN (@Ip, @Host)";

            return new DataBase().QueryBool(query, new Dictionary<string, object?>()
            {
                {"Ip", website.IpAddress},
                {"Host", website.Host}
            });
        }

        public static bool Block(Website website)
        {
            var ipBlockUntil = CalculateIpBlockUntil();
            var hostBlockUntil = CalculateHostBlockUntil(website);
            string query =
                "IF EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + WEBSITES_BLOCKER + " " +
                "   WHERE IpOrHost = @Ip)" +
                "BEGIN" +
                "   UPDATE " + WEBSITES_BLOCKER + " " +
                "   SET BlockUntil = @IpBlockUntil " +
                "   WHERE IpOrHost = @Ip;" +
                "END " +
                "ELSE BEGIN " +
                "   INSERT INTO " + WEBSITES_BLOCKER + " (IpOrHost, BlockUntil) " +
                "   VALUES (@Ip, @IpBlockUntil) " +
                "END " +
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
                {"Ip", website.IpAddress},
                {"IpBlockUntil", ipBlockUntil},
                {"Host", website.Host},
                {"HostBlockUntil", hostBlockUntil},
            });
        }

        public static bool Clean()
        {
            string query = "DELETE FROM [WEBSITES_BLOCKER] WHERE BlockUntil < GETDATE()";
            return new DataBase().Query(query);
        }

        private static DateTime CalculateIpBlockUntil()
        {
            int secconds = RandomSecconds();
            return DateTime.Now.AddSeconds(secconds);
        }

        private static DateTime CalculateHostBlockUntil(Website website)
        {
            int randomSecconds = RandomSecconds();
            int crawDelay = website.CrawlDelay();
            int secconds = Math.Max(randomSecconds, crawDelay);
            return DateTime.Now.AddSeconds(secconds);
        }

        private static int RandomSecconds()
        {
            return Random.Shared.Next(MinSecconds, MaxSecconds);
        }
    }
}
