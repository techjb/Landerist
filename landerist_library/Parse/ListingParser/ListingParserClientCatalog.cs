using System.Diagnostics.CodeAnalysis;
namespace landerist_library.Parse.ListingParser;

public sealed class ListingParserClientCatalog
{
    private readonly IReadOnlyDictionary<LLMProvider, IListingParserClient>
        _clients;

    public ListingParserClientCatalog(
        IEnumerable<IListingParserClient> clients)
    {
        ArgumentNullException.ThrowIfNull(clients);
        Dictionary<LLMProvider, IListingParserClient> catalog = [];
        foreach (IListingParserClient client in clients)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (!catalog.TryAdd(client.Provider, client))
            {
                throw new ArgumentException(
                    $"Duplicate listing parser client: {client.Provider}.",
                    nameof(clients));
            }
        }

        _clients = catalog;
    }

    public bool TryGet(
        LLMProvider provider,
        [NotNullWhen(true)] out IListingParserClient? client) =>
        _clients.TryGetValue(provider, out client);
}
