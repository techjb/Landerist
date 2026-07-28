using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Listings;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class SqlListingEnricherTests
{
    [Fact]
    public void Enrich_DelegatesLocationEnrichmentThroughPort()
    {
        RecordingLocationEnricher location = new();
        SqlListingEnricher enricher = new(new RecordingDatabase(), location);
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));
        Listing listing = new();

        enricher.Enrich(page, listing);

        Assert.Same(page, location.Page);
        Assert.Same(listing, location.Listing);
    }

    private sealed class RecordingLocationEnricher : IListingLocationEnricher
    {
        public Page? Page { get; private set; }
        public Listing? Listing { get; private set; }

        public void EnrichLocation(Page page, Listing listing)
        {
            Page = page;
            Listing = listing;
        }
    }
}