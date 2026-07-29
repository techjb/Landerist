using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_domain.Parsing.Materialization;
using landerist_library.Pages;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class ListingMaterializationRulesTests
{
    private static readonly ListingMaterializationRules Rules = new(
        MaxPublishedAgeYears: 2,
        MinPropertySize: 10,
        MaxPropertySize: 20,
        MinLandSize: 100,
        MaxLandSize: 200,
        MinConstructionYear: 1990,
        MaxConstructionYearsFromNow: 1,
        MinFloors: 1,
        MaxFloors: 3,
        MinBedrooms: 2,
        MaxBedrooms: 4,
        MinBathrooms: 1,
        MaxBathrooms: 2,
        MinParkings: 0,
        MaxParkings: 1,
        MediaEnabled: false);

    [Fact]
    public void Parse_RespectsCustomNumericBounds()
    {
        Anuncio announcement = new()
        {
            TamañoDelInmueble = 15,
            TamañoDeLaParcela = 250,
            AñoDeConstrucción = 2027,
            PlantasDelEdificio = 4,
            NúmeroDeDormitorios = 3,
            NúmeroDeBaños = 0,
            NúmeroDeParkings = 1
        };

        var result = Parse(announcement);

        Assert.NotNull(result.listing);
        Assert.Equal(15, result.listing.propertySize);
        Assert.Null(result.listing.landSize);
        Assert.Equal(2027, result.listing.constructionYear);
        Assert.Null(result.listing.floors);
        Assert.Equal(3, result.listing.bedrooms);
        Assert.Null(result.listing.bathrooms);
        Assert.Equal(1, result.listing.parkings);
    }

    [Theory]
    [InlineData("2024-07-28", 2024)]
    [InlineData("2023-07-26", 2026)]
    public void Parse_UsesInjectedClockForListingDateValidation(
        string published,
        int expectedYear)
    {
        Anuncio announcement = new() { FechaDePublicación = published };

        var result = Parse(announcement);

        Assert.Equal(expectedYear, result.listing?.listingDate?.Year);
    }

    private static (
        PageType pageType,
        landerist_orels.ES.Listing? listing) Parse(Anuncio announcement)
    {
        StructuredOutputEs output = new() { Anuncio = announcement };
        StructuredOutputEsParser parser = new(
            output,
            Rules,
            new FixedTimeProvider(
                new DateTimeOffset(
                    2026,
                    7,
                    27,
                    12,
                    0,
                    0,
                    TimeSpan.Zero)),
            new StructuredOutputMaterializationOperations(
                value => value.Trim(),
                value => value.Replace(" ", string.Empty),
                _ => true,
                _ => true,
                _ => true,
                (_, _, _, _) => { },
                (_, _, _) => { }));
        WebsiteAccessServices websiteAccess = new(
            new StubWebsiteRobotsPolicy(),
            new StubTransportFactory());

        return parser.Parse(
            new Page("https://example.com/listing/1"),
            websiteAccess);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class StubTransportFactory : IHttpClientTransportFactory
    {
        public HttpClient Create(
            bool useProxy,
            TimeSpan timeout,
            bool allowAutoRedirect = true) =>
            throw new NotSupportedException();
    }
}
