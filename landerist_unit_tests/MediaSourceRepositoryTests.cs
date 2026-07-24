using landerist_library.Infrastructure.Sql;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_unit_tests;

public sealed class MediaSourceRepositoryTests
{
    [Fact]
    public void SourceInsert_DelegatesSourceValues()
    {
        RecordingDatabase database = new();
        SourceRepository repository = new(database);
        Listing listing = CreateListing();
        listing.sources.Add(new Source
        {
            sourceName = "Agency",
            sourceUrl = new Uri("https://example.com/listing/1"),
            sourceGuid = "source-guid"
        });

        repository.Insert(listing);

        Assert.Equal("listing-guid", database.LastParameters!["ListingGuid"]);
        Assert.Equal("Agency", database.LastParameters["SourceName"]);
        Assert.Equal("https://example.com/listing/1", database.LastParameters["SourceUrl"]);
        Assert.Equal("source-guid", database.LastParameters["SourceGuid"]);
        Assert.Contains("INSERT INTO [ES_SOURCES]", database.LastQuery);
    }

    [Fact]
    public void SourceDelete_DelegatesGuidAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SourceRepository repository = new(database);

        bool result = repository.Delete("listing-guid");

        Assert.True(result);
        Assert.Equal("listing-guid", database.LastParameters!["listingGuid"]);
        Assert.Contains("DELETE FROM [ES_SOURCES]", database.LastQuery);
    }

    [Fact]
    public void SourceDeleteAll_DelegatesWithoutParameters()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SourceRepository repository = new(database);

        bool result = repository.DeleteAll();

        Assert.True(result);
        Assert.Null(database.LastParameters);
        Assert.Equal("DELETE FROM [ES_SOURCES]", database.LastQuery);
    }

    [Fact]
    public void SourceGetSources_ReturnsTableAndDelegatesGuid()
    {
        RecordingDatabase database = new();
        SourceRepository repository = new(database);
        Listing listing = CreateListing();

        DataTable result = repository.GetSources(listing);

        Assert.Same(database.TableResult, result);
        Assert.Equal("listing-guid", database.LastParameters!["listingGuid"]);
        Assert.Contains("SELECT *", database.LastQuery);
    }

    [Fact]
    public void MediaInsert_DelegatesMediaValues()
    {
        RecordingDatabase database = new();
        MediaRepository repository = new(database);
        Listing listing = CreateListing();
        listing.media.Add(new Media
        {
            mediaType = MediaType.image,
            title = "Front",
            url = new Uri("https://example.com/image.jpg")
        });

        repository.Insert(listing);

        Assert.Equal("listing-guid", database.LastParameters!["listingGuid"]);
        Assert.Equal("image", database.LastParameters["mediaType"]);
        Assert.Equal("Front", database.LastParameters["title"]);
        Assert.Equal("https://example.com/image.jpg", database.LastParameters["url"]);
        Assert.Contains("INSERT INTO [ES_MEDIA]", database.LastQuery);
    }

    [Fact]
    public void MediaDelete_DelegatesGuidAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        MediaRepository repository = new(database);

        bool result = repository.Delete("listing-guid");

        Assert.True(result);
        Assert.Equal("listing-guid", database.LastParameters!["listingGuid"]);
        Assert.Contains("DELETE FROM [ES_MEDIA]", database.LastQuery);
    }

    [Fact]
    public void MediaDeleteAll_DelegatesWithoutParameters()
    {
        RecordingDatabase database = new() { QueryResult = true };
        MediaRepository repository = new(database);

        bool result = repository.DeleteAll();

        Assert.True(result);
        Assert.Null(database.LastParameters);
        Assert.Equal("DELETE FROM [ES_MEDIA]", database.LastQuery);
    }

    [Fact]
    public void MediaGetMedia_ReturnsTableAndDelegatesGuid()
    {
        RecordingDatabase database = new();
        MediaRepository repository = new(database);
        Listing listing = CreateListing();

        DataTable result = repository.GetMedia(listing);

        Assert.Same(database.TableResult, result);
        Assert.Equal("listing-guid", database.LastParameters!["listingGuid"]);
        Assert.Contains("SELECT *", database.LastQuery);
    }

    private static Listing CreateListing()
    {
        return new Listing
        {
            guid = "listing-guid",
            listingStatus = ListingStatus.published,
            operation = Operation.sell,
            propertyType = PropertyType.home
        };
    }
}
