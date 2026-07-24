using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Collections.Concurrent;

namespace landerist_library.Pages;

public partial class Pages
{
    public static bool DeleteAll() => Maintenance.DeleteAll();

    public static void Delete(PageType pageType) => Delete(GetPages(pageType));

    public static void DeleteDuplicateUriQuery()
    {
        IReadOnlyList<string> uris = Queries.GetUris();
        int counter = 0;
        int total = uris.Count;
        var pages = new ConcurrentBag<Page>();

        Parallel.ForEach(uris, uriString =>
        {
            var uri = new Uri(uriString);
            var newUri = Uris.CleanUri(uri);
            int current = Interlocked.Increment(ref counter);

            if (newUri != uri)
            {
                Page page = LoadOrCreate(uri);
                new Indexer(page).Insert(page.Uri);
                pages.Add(page);
            }

            Console.WriteLine(current + "/" + total);
        });

        Delete([.. pages]);
    }

    public static void DeleteListingsHttpStatusCodeError() =>
        Delete([.. Queries.GetListingsWithHttpStatusCodeError()]);

    public static void DeleteListingsResponseBodyRepeated()
    {
        IReadOnlyList<Page> pages = Queries.GetListingsWithParserInputHash();
        HashSet<string> hashes = [];
        List<Page> repeated = [];
        foreach (Page page in pages)
        {
            if (page.ListingParserInputHash is not null &&
                !hashes.Add(page.ListingParserInputHash))
            {
                repeated.Add(page);
            }
        }
        Delete(repeated);
    }

    public static void DeleteUrisLikePrint() =>
        Delete([.. Queries.GetUrisLikePrint()]);

    public static void DeleteProhibitedUris() =>
        Delete([.. Queries.GetPagesWithProhibitedUris(ProhibitedUrls.Prohibited_ES)]);

    public static void Delete(List<Page> pages)
    {
        Console.WriteLine("Deleting " + pages.Count + " pages..");
        int counter = 0;
        int errors = 0;
        int total = pages.Count;
        Parallel.ForEach(pages, page =>
        {
            Console.WriteLine(page.Uri);
            if (global::landerist_library.Pages.Pages.Delete(page))
            {
                Interlocked.Increment(ref counter);
            }
            else
            {
                Interlocked.Increment(ref errors);
            }
            Console.WriteLine($"Deleted {counter}/{total} Errors: {errors}");
        });
    }

    public static void DeleteUnpublishedListings()
    {
        DateTime unlistingDate = DateTime.Now.AddDays(-Config.DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS);
        var listings = ES_Listings.GetUnpublishedListings(unlistingDate);
        DeleteListings(listings);
    }

    private static void DeleteListings(SortedSet<Listing> listings)
    {
        int counter = 0;
        int deleted = 0;
        int errors = 0;
        Parallel.ForEach(listings, listing =>
        {
            Interlocked.Increment(ref counter);
            foreach (var source in listing.sources)
            {
                Page page = LoadOrCreate(source.sourceUrl);
                if (global::landerist_library.Pages.Pages.DeleteListing(page))
                {
                    Interlocked.Increment(ref deleted);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            }

            Console.WriteLine(counter + "/" + listings.Count + " Deleted: " + deleted);
        });
    }
}
