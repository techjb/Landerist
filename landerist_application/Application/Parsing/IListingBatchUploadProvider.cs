using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public interface IListingBatchUploadProvider
{
    BatchProvider Provider { get; }
    string? Serialize(Page page, string userInput);
    string? UploadFile(string filePath);
    string? CreateBatch(string fileId);
}

public sealed class ListingBatchUploadProviderCatalog
{
    private readonly IReadOnlyDictionary<BatchProvider, IListingBatchUploadProvider>
        _providers;

    public ListingBatchUploadProviderCatalog(
        IEnumerable<IListingBatchUploadProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        try
        {
            _providers = providers.ToDictionary(provider => provider.Provider);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException(
                "Only one batch upload provider can be registered per LLM provider.",
                nameof(providers),
                exception);
        }
    }

    public IListingBatchUploadProvider GetRequired(BatchProvider provider) =>
        _providers.TryGetValue(provider, out IListingBatchUploadProvider? selected)
            ? selected
            : throw new InvalidOperationException(
                $"No batch upload provider is registered for {provider}.");
}
