using landerist_library.Configuration;
using landerist_library.Database;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        public static List<Page> SelectWaitingStatusAIRequest(int topRows, WaitingStatus waitingStatusTo, int tokenCount, bool isMaxTokenCount)
        {
            return SelectWaitingStatus(topRows, WaitingStatus.waiting_ai_request, waitingStatusTo, tokenCount, isMaxTokenCount);
        }

        private static List<Page> SelectWaitingStatus(int topRows, WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo, int tokenCount, bool isMaxTokenCount)
        {
            string query =
                "BEGIN TRANSACTION; " +
                "WITH PagesToUpdate AS ( " +
                "SELECT TOP (" + topRows + ") " + PAGES + ".[UriHash] " +
                "FROM " + PAGES + " " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " ON " + PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] " +
                "WHERE " + PAGES + ".[WaitingStatus] = @waitingStatusFrom AND [TokenCount] " + (isMaxTokenCount ? "<=" : ">") + " " + tokenCount + " " +
                "ORDER BY " + PAGES + ".[Updated] ASC ) " +
                "UPDATE " + PAGES + " " +
                "SET [WaitingStatus] = @waitingStatusTo " +
                "OUTPUT " + SelectColumns("INSERTED") + " " +
                "FROM " + PAGES + " " +
                "INNER JOIN PagesToUpdate ON " + PAGES + ".[UriHash] = PagesToUpdate.[UriHash] " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " ON " + PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] " +
                "COMMIT TRANSACTION;";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"WaitingStatusFrom", waitingStatusFrom.ToString() },
                {"waitingStatusTo", waitingStatusTo.ToString() },
            });
            return GetPages(dataTable);
        }

        public static bool UpdateWaitingStatusAIRequest(string uriHash)
        {
            return UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_request);
        }

        public static bool UpdateWaitingStatusAIResponse(string uriHash)
        {
            return UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_response);
        }

        public static bool UpdateWaitingStatus(string uriHash, WaitingStatus waitingStatus)
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [WaitingStatus] = @WaitingStatus " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"WaitingStatus", waitingStatus.ToString() },
                {"UriHash", uriHash }
            });
        }

        public static bool UpdateWaitingStatus(WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo)
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [WaitingStatus] = @WaitingStatusTo " +
                "WHERE [WaitingStatus] = @WaitingStatusFrom";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                { "WaitingStatusFrom", waitingStatusFrom.ToString() },
                { "WaitingStatusTo", waitingStatusTo.ToString() }
            });
        }

        public static void CleanLockedBy()
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [LockedBy] = NULL " +
                "WHERE [LockedBy] = @LockedBy";

            new DataBase().Query(query, new Dictionary<string, object?>()
            {
                { "LockedBy", Config.MACHINE_NAME }
            });
        }
    }
}
