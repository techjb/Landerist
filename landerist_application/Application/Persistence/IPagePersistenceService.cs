using landerist_library.Pages;

namespace landerist_library.Application.Persistence;

public interface IPagePersistenceService
{
    bool Insert(Page page);

    bool Update(Page page);

    Task<bool> UpdateAsync(
        Page page,
        CancellationToken cancellationToken = default);

    bool UpdateNextScrape(Page page);

    bool Delete(Page page);

    bool ListingParserInputExistsOnAnotherListing(Page page);
}
