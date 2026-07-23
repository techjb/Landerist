using landerist_library.Application.Pages;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.PageServices;

public sealed class SqlPageWaitingStatusService : IPageWaitingStatusService
{
    private readonly PageMaintenanceRepository _repository;

    public SqlPageWaitingStatusService(PageMaintenanceRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public List<Page> SelectAIRequest(
        int topRows,
        WaitingStatus waitingStatusTo,
        int tokenCount,
        bool isMaxTokenCount)
    {
        DataTable rows = _repository.SelectWaitingStatus(
            topRows,
            WaitingStatus.waiting_ai_request,
            waitingStatusTo,
            tokenCount,
            isMaxTokenCount);
        return MapPages(rows);
    }

    public bool UpdateAIRequest(string uriHash) =>
        _repository.UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_request);

    public bool UpdateAIResponse(string uriHash) =>
        _repository.UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_response);

    public bool Update(WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo) =>
        _repository.UpdateWaitingStatus(waitingStatusFrom, waitingStatusTo);

    private static List<Page> MapPages(DataTable rows)
    {
        List<Page> pages = [];
        foreach (DataRow row in rows.Rows)
        {
            var website = WebsiteDataMapper.Map(row);
            pages.Add(PageDataMapper.Map(row, website));
        }
        return pages;
    }
}