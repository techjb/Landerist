using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        public static bool Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            return global::landerist_library.Pages.Pages.Insert(page);
        }

        public static void UpdateInvalidCadastastralReferences()
        {
            var pages = GetPages();
            int total = pages.Count;
            int updated = 0;
            int counter = 0;

            foreach (var page in pages)
            {
                Console.WriteLine(counter++ + "/" + total);
                var listing = global::landerist_library.Pages.Pages.GetListing(page, false, false);
                if (listing != null && listing.cadastralReference != null)
                {
                    if (!Validate.CadastralReference(listing.cadastralReference))
                    {
                        listing.cadastralReference = null;
                        updated++;
                        if (ListingMaintenance.Update(listing))
                        {
                            Console.WriteLine("UPDATED: " + updated++);
                        }
                    }
                }
            }
            Console.WriteLine(updated + "/" + total);
        }

        public static void UpdateNextScrape()
        {
            int total = CountPages();
            int updated = 0;
            int counter = 0;
            int errors = 0;

            foreach (var pages in GetPageBatches())
            {
                Parallel.ForEach(pages, page =>
                {
                    Interlocked.Increment(ref counter);
                    DateTime calculationDate = page.LastScrape ?? page.Inserted;
                    page.NextScrape = PageNextScrapeCalculator.Calculate(page, calculationDate, GetListingStatus(page));

                    if (global::landerist_library.Pages.Pages.UpdateNextScrape(page))
                    {
                        Interlocked.Increment(ref updated);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                    Console.WriteLine(counter + "/" + total + " updated: " + updated + " errors: " + errors);
                });
            }

            Console.WriteLine(counter + "/" + total + " updated: " + updated + " errors: " + errors);
        }

        public static bool RemoveListingParserInputHash(PageType pageType)
        {
            return Maintenance.RemoveListingParserInputHash(pageType);
        }

        public static bool RemoveListingParserInputHashToAll()
        {
            return Maintenance.RemoveListingParserInputHash();
        }
    }
}
