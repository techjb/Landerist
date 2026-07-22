using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;

namespace landerist_library.Pages;

public partial class Pages
{
    private static readonly PagePersistenceService Persistence = new(new PageRepository(new DataBase()));

    public static bool Insert(Page page) => Persistence.Insert(page);

    public static bool Update(Page page) => Persistence.Update(page);

    public static bool UpdateNextScrape(Page page) => Persistence.UpdateNextScrape(page);

    public static bool Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        bool success = Persistence.Delete(page);
        return success &&
            ES_Listings.Delete(page.UriHash) &&
            ES_Media.Delete(page.UriHash) &&
            ES_Sources.Delete(page.UriHash);
    }

    public static bool DeleteListing(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Listing? listing = ES_Listings.GetListing(page, false, false);
        if (listing is null || !ES_Listings.Delete(listing))
        {
            return false;
        }

        ES_Media.Delete(listing);
        ES_Sources.Delete(listing);
        return true;
    }

    public static bool ListingParserInputExistsOnAnotherListing(Page page) =>
        Persistence.ListingParserInputExistsOnAnotherListing(page);
}