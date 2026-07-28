using landerist_library.Application.Pages;
using landerist_library.Infrastructure.Sql;

namespace landerist_library.Infrastructure.PageServices;

public sealed class SqlPageDeletionService : IPageDeletionService
{
    private readonly PageMaintenanceRepository _repository;

    public SqlPageDeletionService(PageMaintenanceRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public bool DeleteByHost(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        return _repository.DeleteByHost(host);
    }
}
