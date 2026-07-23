using landerist_library.Pages;

namespace landerist_library.Application.Persistence;

public sealed class PagePersistenceService : IPagePersistenceService
{
    private readonly IPageRepository _repository;

    public PagePersistenceService(IPageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
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
            Logs.Log.WriteError(nameof(PagePersistenceService), $"Failed to update page: {page.Uri}. Message: {exception.Message}");
        }
        return updated;
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

    public bool ListingParserInputExistsOnAnotherListing(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _repository.ListingParserInputExistsOnAnotherListing(page.Host, page.UriHash, page.ListingParserInputHash);
    }
}