using landerist_library.Pages;

namespace landerist_library.Application.Listings;

public interface IListingUnpublishPolicy
{
    ListingUnpublishDecision Evaluate(Page page);
}
