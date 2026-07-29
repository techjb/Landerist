using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class ListingRepositoryTests
{
    [Fact]
    public void Insert_DelegatesListingAndHostParameters()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);
        Listing listing = CreateListing();

        bool result = repository.Insert(
            listing,
            "example.com",
            unpublishDecision: null,
            out Exception? exception);

        Assert.True(result);
        Assert.Null(exception);
        Assert.Equal("listing-guid", database.LastParameters!["guid"]);
        Assert.Equal("example.com", database.LastParameters["host"]);
        Assert.Equal("published", database.LastParameters["listingStatus"]);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public async Task InsertAsync_UsesAsyncDatabaseWrite()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);

        bool result = await repository.InsertAsync(CreateListing(), "example.com", null);

        Assert.True(result);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public async Task UpdateAsync_UsesAsyncDatabaseWrite()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);

        bool result = await repository.UpdateAsync(CreateListing());

        Assert.True(result);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Contains("UPDATE", database.LastQuery);
    }

    [Fact]
    public void Update_DelegatesListingParameters()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);
        Listing listing = CreateListing();

        bool result = repository.Update(listing);

        Assert.True(result);
        Assert.Equal("listing-guid", database.LastParameters!["guid"]);
        Assert.Equal("sell", database.LastParameters["operation"]);
        Assert.Equal("home", database.LastParameters["propertyType"]);
        Assert.Contains("UPDATE", database.LastQuery);
    }

    [Fact]
    public void Delete_DelegatesGuid()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);

        bool result = repository.Delete("listing-guid");

        Assert.True(result);
        Assert.Equal("listing-guid", database.LastParameters!["guid"]);
        Assert.Contains("DELETE FROM", database.LastQuery);
    }

    [Fact]
    public void UpdateAddress_DelegatesLocationValues()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);

        bool result = repository.UpdateAddress(
            "listing-guid",
            40.4,
            -3.7,
            true,
            "test-resolver");

        Assert.True(result);
        Assert.Equal(40.4, database.LastParameters!["latitude"]);
        Assert.Equal(-3.7, database.LastParameters["longitude"]);
        Assert.Equal(true, database.LastParameters["locationIsAccurate"]);
        Assert.Equal("test-resolver", database.LastParameters["locationResolver"]);
    }

    [Fact]
    public async Task UpdateAddressAsync_UsesAsyncDatabaseWrite()
    {
        RecordingDatabase database = new() { QueryResult = true };
        ListingRepository repository = new(database);

        bool result = await repository.UpdateAddressAsync(
            "listing-guid", 40.4, -3.7, true, "test-resolver", CancellationToken.None);

        Assert.True(result);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Equal("listing-guid", database.LastParameters!["guid"]);
        Assert.Equal("test-resolver", database.LastParameters["locationResolver"]);
    }

        [Fact]
    public void GetListingsByHost_AddsOptionalStatusFilter()
    {
        RecordingDatabase database = new();
        ListingQueryRepository repository = new(database);

        repository.GetListings("example.com", ListingStatus.published);

        Assert.Contains("[ListingStatus] = @ListingStatus", database.LastQuery);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("published", database.LastParameters["ListingStatus"]);
    }

    [Fact]
    public void GetListingsByDate_DelegatesRange()
    {
        RecordingDatabase database = new();
        ListingQueryRepository repository = new(database);
        DateOnly from = new(2026, 1, 1);
        DateOnly to = new(2026, 1, 31);

        repository.GetListings(ListingStatus.unpublished, from, to);

        Assert.Equal("unpublished", database.LastParameters!["ListingStatus"]);
        Assert.Equal(from, database.LastParameters["DateFrom"]);
        Assert.Equal(to, database.LastParameters["DateTo"]);
    }

    [Fact]
    public void CountByHostAndStatus_UsesStatisticsRepository()
    {
        RecordingDatabase database = new() { QueryIntResult = 7 };
        ListingStatisticsRepository repository = new(database);

        int result = repository.Count("example.com", ListingStatus.published);

        Assert.Equal(7, result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("published", database.LastParameters["ListingStatus"]);
    }

    [Fact]
    public void CountWithImages_DelegatesMediaType()
    {
        RecordingDatabase database = new() { QueryIntResult = 3 };
        ListingStatisticsRepository repository = new(database);

        int result = repository.CountWithImages("example.com", ListingStatus.published);

        Assert.Equal(3, result);
        Assert.Equal("image", database.LastParameters!["MediaType"]);
        Assert.Contains("EXISTS", database.LastQuery);
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
