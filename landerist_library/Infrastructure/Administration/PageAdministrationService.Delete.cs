using landerist_library.Pages;
using landerist_library.Configuration;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Collections.Concurrent;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class PageAdministrationService
{
    public bool DeleteAll() => Maintenance.DeleteAll();

    public void Delete(PageType pageType) => Delete([.. GetPages(pageType)]);

    public void DeleteDuplicateUriQuery()
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

    public void DeleteListingsHttpStatusCodeError() =>
        Delete([.. Queries.GetListingsWithHttpStatusCodeError()]);

    public void DeleteListingsResponseBodyRepeated()
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

    public void DeleteUrisLikePrint() =>
        Delete([.. Queries.GetUrisLikePrint()]);

    public void DeleteProhibitedUris() =>
        Delete([.. Queries.GetPagesWithProhibitedUris(ProhibitedUrls.Prohibited_ES)]);

    public void Delete(List<Page> pages)
    {
        Console.WriteLine("Deleting " + pages.Count + " pages..");
        int counter = 0;
        int errors = 0;
        int total = pages.Count;
        Parallel.ForEach(pages, page =>
        {
            Console.WriteLine(page.Uri);
            if (Delete(page))
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

    public void DeleteUnpublishedListings()
    {
        DateTime unlistingDate = DateTime.Now.AddDays(-Config.DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS);
        IReadOnlyCollection<Listing> listings =
            ListingQueries.GetUnpublishedBefore(unlistingDate);
        DeleteListings(listings);
    }

    private void DeleteListings(IReadOnlyCollection<Listing> listings)
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
                if (DeleteListing(page))
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

