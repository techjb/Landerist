using landerist_library.Application.Listings;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Listings;

public sealed class LegacyListingUnpublishPolicy : IListingUnpublishPolicy
{
    public ListingUnpublishDecision Evaluate(Page page) =>
        new ListingUnpublishEvaluator(page).Evaluate();
}
