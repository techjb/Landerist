using landerist_library.Pages;

namespace landerist_library.Application.Pages;

public interface IPageWaitingStatusService
{
    List<Page> SelectAIRequest(
        int topRows,
        WaitingStatus waitingStatusTo,
        int tokenCount,
        bool isMaxTokenCount);

    bool UpdateAIRequest(string uriHash);

    bool UpdateAIResponse(string uriHash);

    bool Update(WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo);
}