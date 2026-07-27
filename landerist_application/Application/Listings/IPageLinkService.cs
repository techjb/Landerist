using landerist_library.Pages;

namespace landerist_library.Application.Listings;

public interface IPageLinkService
{
    Uri? Resolve(Page sourcePage, string? url);

    void Index(Page sourcePage, Uri destinationUri);
}
