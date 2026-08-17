using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class StructuredOutputFieldMappingTests
{
    [Fact]
    public void Parse_WithoutAnnouncement_ReturnsNotListing()
    {
        var result = CreateParser(new StructuredOutputEs()).Parse(
            CreatePage(),
            CreateWebsiteAccess());

        Assert.Equal(PageType.NotListingByParser, result.pageType);
        Assert.Null(result.listing);
    }

    [Theory]
    [InlineData(true, "600123123")]
    [InlineData(false, null)]
    public void Parse_MapsPhoneOnlyWhenValidatorAcceptsIt(
        bool valid,
        string? expected)
    {
        StructuredOutputEs output = new()
        {
            Anuncio = new Anuncio { TeléfonoDeContacto = " 600123123 " }
        };

        var result = CreateParser(output, validatePhone: _ => valid).Parse(
            CreatePage(),
            CreateWebsiteAccess());

        Assert.Equal(expected, result.listing?.contactPhone);
    }

    [Theory]
    [InlineData(true, "info@example.com")]
    [InlineData(false, null)]
    public void Parse_RemovesEmailSpacesBeforeValidation(
        bool valid,
        string? expected)
    {
        StructuredOutputEs output = new()
        {
            Anuncio = new Anuncio { EmailDeContacto = " info @example.com " }
        };

        var result = CreateParser(output, validateEmail: _ => valid).Parse(
            CreatePage(),
            CreateWebsiteAccess());

        Assert.Equal(expected, result.listing?.contactEmail);
    }

    [Fact]
    public void Announcement_IsExposedAsReadOnlyState()
    {
        var property = typeof(StructuredOutputEsParser).GetProperty("Anuncio");

        Assert.NotNull(property);
        Assert.False(property.CanWrite);
        Assert.Null(typeof(StructuredOutputEsParser).GetField("Anuncio"));
    }

    private static StructuredOutputEsParser CreateParser(
        StructuredOutputEs output,
        Func<string?, bool>? validatePhone = null,
        Func<string?, bool>? validateEmail = null) =>
        new(
            output,
            ListingMaterializationRules.Default with { MediaEnabled = false },
            TimeProvider.System,
            new StructuredOutputMaterializationOperations(
                value => value.Trim(),
                value => value.Replace(" ", string.Empty),
                validatePhone ?? (_ => true),
                validateEmail ?? (_ => true),
                _ => true,
                (_, _, _, _) => { },
                (_, _, _) => { }));

    private static Page CreatePage() =>
        new("https://example.com/listing/1");

    private static WebsiteAccessServices CreateWebsiteAccess() =>
        new(new StubWebsiteRobotsPolicy(), new UnsupportedTransportFactory());

    private sealed class UnsupportedTransportFactory : IHttpClientTransportFactory
    {
        public HttpClient Create(
            bool useProxy,
            TimeSpan timeout,
            bool allowAutoRedirect = true) =>
            throw new NotSupportedException();
    }
}
