using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public sealed class PageBatchSelector : IPageBatchSelector
{
    private readonly IPageSelectionRepository _repository;
    private readonly PageSelectionOptions _options;

    public PageBatchSelector(
        IPageSelectionRepository repository,
        PageSelectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(options);

        _repository = repository;
        _options = options;
    }

    public IReadOnlyList<Page> Select()
    {
        _repository.CleanLockedPages();
        var candidates = _repository.GetScrapePages(_options.MaximumPages);
        List<Page> selected = [];
        HashSet<string> selectedUriHashes = [];
        Dictionary<string, int> pagesPerHost = [];

        foreach (var page in candidates)
        {
            if (selected.Count >= _options.MaximumPages)
            {
                break;
            }

            if (!selectedUriHashes.Add(page.UriHash))
            {
                continue;
            }

            var host = page.Website.Host;
            pagesPerHost.TryGetValue(host, out var hostPageCount);
            if (hostPageCount >= _options.MaximumPagesPerHost)
            {
                continue;
            }

            selected.Add(page);
            pagesPerHost[host] = hostPageCount + 1;
        }

        if (_options.EnforceMinimumPages && selected.Count < _options.MinimumPages)
        {
            return [];
        }

        return selected;
    }
}
