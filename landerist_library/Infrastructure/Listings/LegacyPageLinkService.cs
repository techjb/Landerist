using landerist_library.Application.Listings;
using landerist_library.Index;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Listings;

public sealed class LegacyPageLinkService : IPageLinkService
{
    public Uri? Resolve(Page sourcePage, string? url) =>
        new Indexer(sourcePage).GetUri(url);

    public void Index(Page sourcePage, Uri destinationUri) =>
        new Indexer(sourcePage).Insert(destinationUri);
}
