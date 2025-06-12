using landerist_library.Websites;

namespace landerist_library.Parse.Location
{
    public class LauIdParser(Page page, landerist_orels.ES.Listing listing)
    {
        private readonly Page Page = page;

        private readonly landerist_orels.ES.Listing Listing = listing;

        public void SetLauId()
        {
            switch (Page.Website.CountryCode)
            {
                // More precise map
                case CountryCode.ES:
                    {
                        Listing.lauId = Delimitations.CNIGParser.GetNatCode(Listing);
                    }
                    break;
                default:
                    {
                        Listing.lauId = Delimitations.LAUParser.GetId(Listing);
                    }
                    break;
            }
        }

        public static void SetLauIdToAllListings()
        {
            var listings = Database.ES_Listings.GetAll(false, false);
            var total = listings.Count;
            var count = 0;
            var updated = 0;
            Parallel.ForEach(listings, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, listing =>
            {
                Interlocked.Increment(ref count);
                Console.WriteLine("Setting LAU ID to listing " + count + "/" + total + " Updated: " + updated);
                if (listing.lauId != null)
                {
                    return;
                }
                var lauId = Delimitations.CNIGParser.GetNatCode(listing);
                if (lauId is null)
                {
                    return;
                }
                listing.lauId = lauId;
                Database.ES_Listings.Update(listing);
                Interlocked.Increment(ref updated);
            });
        }
    }
}
