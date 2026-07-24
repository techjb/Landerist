using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_unit_tests;

public sealed class ListingPersistenceAdaptersTests
{
    [Fact]
    public void ListingQueryService_MapsListingWithoutExposingDataTable()
    {
        RecordingDatabase database = new();
        AddListingRow(database.TableResult);
        SqlListingQueryService queries = new(
            new ListingQueryRepository(database),
            new MediaRepository(database),
            new SourceRepository(database));
        Page page = CreatePage();

        Listing? listing = queries.Get(page, loadMedia: false, loadSources: false);

        Assert.NotNull(listing);
        Assert.Equal("listing-guid", listing.guid);
        Assert.Equal(ListingStatus.published, listing.listingStatus);
        Assert.Equal(Operation.sell, listing.operation);
        Assert.Equal(PropertyType.home, listing.propertyType);
        Assert.Equal(PropertySubtype.flat, listing.propertySubtype);
        Assert.Equal(250000m, listing.price!.amount);
        Assert.Equal(Currency.EUR, listing.price.currency);
        Assert.Equal(new Uri("https://example.com/contact"), listing.contactUrl);
        Assert.Equal(40.42d, listing.latitude);
        Assert.True(listing.locationIsAccurate);
        Assert.Equal((short)2005, listing.constructionYear);
        Assert.Equal(ConstructionStatus.good, listing.constructionStatus);
        Assert.True(listing.terrace);
        Assert.Equal(page.UriHash, database.LastParameters!["Guid"]);
    }

    [Fact]
    public void ListingMaintenanceService_DeletesWholeAggregate()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlListingMaintenanceService maintenance = new(
            new ListingRepository(database),
            new MediaRepository(database),
            new SourceRepository(database));

        bool deleted = maintenance.Delete("listing-guid");

        Assert.True(deleted);
        Assert.Equal(3, database.Calls.Count);
        Assert.Contains("[ES_LISTINGS]", database.Calls[0].Query);
        Assert.Contains("[ES_MEDIA]", database.Calls[1].Query);
        Assert.Contains("[ES_SOURCES]", database.Calls[2].Query);
        Assert.All(database.Calls, call =>
            Assert.Equal("listing-guid", call.Parameters!.Values.Single()));
    }

    private static Page CreatePage()
    {
        Website website = new(new Uri("https://example.com"));
        return new Page(website, new Uri("https://example.com/listing/1"));
    }

    private static void AddListingRow(DataTable table)
    {
        table.Columns.Add("guid", typeof(string));
        table.Columns.Add("listingStatus", typeof(string));
        table.Columns.Add("operation", typeof(string));
        table.Columns.Add("propertyType", typeof(string));

        string[] optionalColumns =
        [
            "listingDate",
            "unlistingDate",
            "propertySubtype",
            "priceAmount",
            "priceCurrency",
            "description",
            "contactName",
            "contactPhone",
            "contactEmail",
            "contactUrl",
            "contactOther",
            "address",
            "lauId",
            "lauName",
            "latitude",
            "longitude",
            "locationIsAccurate",
            "locationResolver",
            "cadastralReference",
            "propertySize",
            "landSize",
            "constructionYear",
            "constructionStatus",
            "energyEfficiencyRating",
            "floors",
            "floor",
            "bedrooms",
            "bathrooms",
            "parkings",
            "terrace",
            "garden",
            "garage",
            "motorbikeGarage",
            "pool",
            "lift",
            "disabledAccess",
            "storageRoom",
            "furnished",
            "nonFurnished",
            "heating",
            "airConditioning",
            "petsAllowed",
            "securitySystems"
        ];
        foreach (string column in optionalColumns)
        {
            table.Columns.Add(column, typeof(object));
        }

        DataRow row = table.NewRow();
        row["guid"] = "listing-guid";
        row["listingStatus"] = ListingStatus.published.ToString();
        row["operation"] = Operation.sell.ToString();
        row["propertyType"] = PropertyType.home.ToString();
        row["propertySubtype"] = PropertySubtype.flat.ToString();
        row["priceAmount"] = 250000m;
        row["priceCurrency"] = Currency.EUR.ToString();
        row["contactUrl"] = "https://example.com/contact";
        row["latitude"] = 40.42d;
        row["locationIsAccurate"] = true;
        row["constructionYear"] = (short)2005;
        row["constructionStatus"] = ConstructionStatus.good.ToString();
        row["terrace"] = true;
        table.Rows.Add(row);
    }
}
