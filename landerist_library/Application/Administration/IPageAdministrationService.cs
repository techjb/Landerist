using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Application.Administration;

public interface IPageAdministrationService
{
    Page LoadOrCreate(Uri uri);
    Page? GetPage(string uriHash);
    IReadOnlyList<Page> GetPages();
    IReadOnlyList<Page> GetPages(PageType pageType);
    IReadOnlyList<string> GetUris();
    Listing? GetListing(Page page, bool loadMedia, bool loadSources);
    bool Insert(Page page);
    bool Insert(Website website, Uri uri);
    bool Update(Page page);
    bool UpdateNextScrape(Page page);
    bool Delete(Page page);
    bool DeleteListing(Page page);
    bool DeleteAll();
    void Delete(PageType pageType);
    void DeleteDuplicateUriQuery();
    void DeleteListingsHttpStatusCodeError();
    void DeleteListingsResponseBodyRepeated();
    void DeleteUrisLikePrint();
    void DeleteProhibitedUris();
    void DeleteUnpublishedListings();
    void UpdateInvalidCadastralReferences();
    void RecalculateNextScrape();
    bool RemoveListingParserInputHash(PageType pageType);
    bool RemoveListingParserInputHashFromAll();
}
