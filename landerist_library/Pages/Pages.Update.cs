using landerist_library.Database;
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
            return page.Insert();
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
                var listing = page.GetListing(false, false);
                if (listing != null && listing.cadastralReference != null)
                {
                    if (!Validate.CadastralReference(listing.cadastralReference))
                    {
                        listing.cadastralReference = null;
                        updated++;
                        if (ES_Listings.Update(listing))
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
                    page.NextScrape = PageNextScrapeCalculator.Calculate(page, calculationDate);

                    if (page.UpdateNextScrape())
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
            string query =
                "UPDATE " + PAGES + " " +
                "SET [ListingParserInputHash] = NULL " +
                "WHERE [PageType] = @PageType";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });
        }

        public static bool RemoveListingParserInputHashToAll()
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [ListingParserInputHash] = NULL";

            return new DataBase().Query(query);
        }
    }
}
