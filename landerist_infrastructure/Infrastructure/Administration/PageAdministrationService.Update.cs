using landerist_library.Pages;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Administration
{
    public sealed partial class PageAdministrationService
    {
        public bool Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            return Insert(page);
        }

        public void UpdateInvalidCadastralReferences()
        {
            var pages = GetPages();
            int total = pages.Count;
            int updated = 0;
            int counter = 0;

            foreach (var page in pages)
            {
                Console.WriteLine(counter++ + "/" + total);
                var listing = GetListing(page, false, false);
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

        public void RecalculateNextScrape()
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

                    if (UpdateNextScrape(page))
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

        public bool RemoveListingParserInputHash(PageType pageType)
        {
            return Maintenance.RemoveListingParserInputHash(pageType);
        }

        public bool RemoveListingParserInputHashFromAll()
        {
            return Maintenance.RemoveListingParserInputHash();
        }
    }
}

