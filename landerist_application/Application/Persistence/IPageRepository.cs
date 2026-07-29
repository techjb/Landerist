using landerist_library.Pages;

namespace landerist_library.Application.Persistence;

public interface IPageRepository
{
    bool Insert(Page page);
    bool Update(Page page, out Exception? exception);
    Task<bool> UpdateAsync(
        Page page,
        CancellationToken cancellationToken = default);
    bool UpdateNextScrape(string uriHash, DateTime? nextScrape);
    bool Delete(string uriHash);
    bool ListingParserInputExistsOnAnotherListing(string host, string uriHash, string? listingParserInputHash);
}