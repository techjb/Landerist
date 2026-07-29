using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Websites;

namespace landerist_domain.Parsing.Materialization;

public sealed record ListingParsingServices
{
    public ListingMaterializationRules MaterializationRules { get; }

    public WebsiteAccessServices WebsiteAccess { get; }

    public TimeProvider TimeProvider { get; }

    public ListingParsingServices(
        ListingMaterializationRules materializationRules,
        WebsiteAccessServices websiteAccess,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(materializationRules);
        ArgumentNullException.ThrowIfNull(websiteAccess);
        ArgumentNullException.ThrowIfNull(timeProvider);
        MaterializationRules = materializationRules;
        WebsiteAccess = websiteAccess;
        TimeProvider = timeProvider;
    }
}
