using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class OrelsListingDeletionService : IListingDeletionService
{
    public void Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Listing? listing = ES_Listings.GetListing(page, false, false);
        if (listing is null || !ES_Listings.Delete(listing))
        {
            return;
        }

        ES_Media.Delete(listing);
        ES_Sources.Delete(listing);
    }
}
