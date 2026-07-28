using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Parsing;

namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchDownloadProvider(
    BatchProvider Provider,
    IListingBatchProvider Client);

public sealed class BatchDownloadProviderCatalog
{
    private readonly IReadOnlyDictionary<BatchProvider, IListingBatchProvider> _providers;

    public BatchDownloadProviderCatalog(IEnumerable<BatchDownloadProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = providers.ToDictionary(item => item.Provider, item => item.Client);
    }

    public IListingBatchProvider GetRequired(BatchProvider provider) =>
        _providers.TryGetValue(provider, out IListingBatchProvider? client)
            ? client
            : throw new InvalidOperationException(
                $"No batch download provider is registered for {provider}.");
}
