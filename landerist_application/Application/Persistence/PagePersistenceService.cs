using landerist_library.Application.Logging;
using landerist_library.Pages;

namespace landerist_library.Application.Persistence;

public sealed class PagePersistenceService : IPagePersistenceService
{
    private readonly IPageRepository _repository;
    private readonly IApplicationLogger _logger;

    public PagePersistenceService(IPageRepository repository, IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public bool Insert(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _repository.Insert(page);
    }

    public bool Update(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        bool updated = _repository.Update(page, out Exception? exception);
        if (!updated && exception is not null)
        {
            LogUpdateFailure(page, exception);
        }
        return updated;
    }

    public async Task<bool> UpdateAsync(
        Page page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);
        try
        {
            return await _repository
                .UpdateAsync(page, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogUpdateFailure(page, exception);
            return false;
        }
    }
    public bool UpdateNextScrape(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _repository.UpdateNextScrape(page.UriHash, page.NextScrape);
    }

    public bool Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _repository.Delete(page.UriHash);
    }

    private void LogUpdateFailure(Page page, Exception exception) =>
        _logger.WriteError(
            nameof(PagePersistenceService),
            $"Failed to update page: {page.Uri}. Message: {exception.Message}");
    public bool ListingParserInputExistsOnAnotherListing(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _repository.ListingParserInputExistsOnAnotherListing(page.Host, page.UriHash, page.ListingParserInputHash);
    }
}