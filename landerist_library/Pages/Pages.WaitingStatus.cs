using landerist_library.Infrastructure.Sql;
using landerist_library.Configuration;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        private static readonly PageMaintenanceRepository MaintenanceRepository = new();

        public static List<Page> SelectWaitingStatusAIRequest(int topRows, WaitingStatus waitingStatusTo, int tokenCount, bool isMaxTokenCount)
        {
            return SelectWaitingStatus(topRows, WaitingStatus.waiting_ai_request, waitingStatusTo, tokenCount, isMaxTokenCount);
        }

        private static List<Page> SelectWaitingStatus(int topRows, WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo, int tokenCount, bool isMaxTokenCount)
        {
            var dataTable = MaintenanceRepository.SelectWaitingStatus(topRows, waitingStatusFrom, waitingStatusTo, tokenCount, isMaxTokenCount);
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
            return MaintenanceRepository.UpdateWaitingStatus(uriHash, waitingStatus);
        }

        public static bool UpdateWaitingStatus(WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo)
        {
            return MaintenanceRepository.UpdateWaitingStatus(waitingStatusFrom, waitingStatusTo);
        }

        public static void CleanLockedBy()
        {
            MaintenanceRepository.CleanLockedBy(Config.MACHINE_NAME);
        }
    }
}
