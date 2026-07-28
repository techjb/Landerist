using landerist_library.Application.Pages;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.PageServices;

public sealed class SqlPageQueryService : IPageQueryService
{
    private const int AllPagesBatchSize = 3000;
    private readonly PageQueryRepository _repository;

    public SqlPageQueryService(PageQueryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public Page? GetByHash(string uriHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uriHash);
        List<Page> pages = Map(_repository.GetPageByUriHash(uriHash));
        return pages.Count == 1 ? pages[0] : null;
    }



    public IEnumerable<IReadOnlyList<Page>> GetBatches(int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        string? lastUriHash = null;
        while (true)
        {
            List<Page> batch = Map(_repository.GetPagesBatch(lastUriHash, batchSize));
            if (batch.Count == 0)
            {
                yield break;
            }

            yield return batch;
            if (batch.Count < batchSize)
            {
                yield break;
            }

            lastUriHash = batch[^1].UriHash;
        }
    }

    public IReadOnlyList<Page> GetByType(PageType pageType) =>
        Map(_repository.GetPagesByPageType(pageType));

    public IReadOnlyList<Page> GetUnknown() => Map(_repository.GetUnknownPageType());

    public IReadOnlyList<Page> GetUnknown(int topRows) =>
        Map(_repository.GetUnknownPageTypeForUpdate(topRows));

    public IReadOnlyList<Page> GetNextScrape(int topRows, bool extendToFillTopRows) =>
        Map(_repository.GetNextScrapeForUpdate(topRows, extendToFillTopRows));

    public IReadOnlyList<Page> GetNextScrapeFuture(int topRows) =>
        Map(_repository.GetNextScrapeFutureForUpdate(topRows));

    public IReadOnlyList<Page> GetRecentlyUnpublishedListings(int topRows) =>
        Map(_repository.GetRecentlyUnpublishedListingsPages(topRows));

    public IReadOnlyList<Page> GetScrapePages(int topRows) =>
        Map(_repository.GetScrapePages(topRows));

    public IReadOnlyList<Page> GetNonScraped(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return Map(_repository.GetNonScrapedPages(website.Host), website);
    }

    public IReadOnlyList<Page> GetUnknown(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return Map(_repository.GetUnknownPageType(website.Host), website);
    }

    public IReadOnlyList<Page> GetUnknownHttpStatusCode() =>
        Map(_repository.GetUnknownHttpStatusCode());

    public IReadOnlyList<string> GetUris() => _repository.GetUris();

    public IReadOnlyList<string> GetUris(bool isListing) => _repository.GetUris(isListing);

    public IReadOnlyList<Page> GetListingsWithHttpStatusCodeError() =>
        Map(_repository.GetListingsWithHttpStatusCodeError());

    public IReadOnlyList<Page> GetListingsWithParserInputHash() =>
        Map(_repository.GetListingsWithParserInputHash());

    public IReadOnlyList<Page> GetUrisLikePrint() => Map(_repository.GetUrisLikePrint());

    public IReadOnlyList<Page> GetPagesWithProhibitedUris(
        IEnumerable<string> prohibitedUriFragments) =>
        Map(_repository.GetPagesWithProhibitedUris(prohibitedUriFragments));

    public int Count() => _repository.CountPages();

    private static List<Page> Map(DataTable rows)
    {
        List<Page> pages = [];
        foreach (DataRow row in rows.Rows)
        {
            Website website = WebsiteDataMapper.Map(row);
            pages.Add(PageDataMapper.Map(row, website));
        }
        return pages;
    }

    private static List<Page> Map(DataTable rows, Website website)
    {
        List<Page> pages = [];
        foreach (DataRow row in rows.Rows)
        {
            pages.Add(PageDataMapper.Map(row, website));
        }
        return pages;
    }
}

public sealed class SqlPageMaintenanceService : IPageMaintenanceService
{
    private readonly PageMaintenanceRepository _repository;

    public SqlPageMaintenanceService(PageMaintenanceRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public bool DeleteAll() => _repository.DeleteAll();

    public bool RemoveListingParserInputHash(PageType? pageType = null) =>
        _repository.RemoveListingParserInputHash(pageType);
}
