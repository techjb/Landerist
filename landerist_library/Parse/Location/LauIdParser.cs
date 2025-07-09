using landerist_library.Database;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.Location
{
    public class LauIdParser(CountryCode countryCode, Listing listing)
    {
        private readonly CountryCode CountryCode = countryCode;

        private readonly Listing Listing = listing;

        public void SetLauIdAndLauName()
        {
            switch (CountryCode)
            {
                // More precise map
                case CountryCode.ES:
                    {
                        var natCodeAndNameUnit = Delimitations.CNIGParser.GetNatCodeAndNameUnit(Listing);
                        if (natCodeAndNameUnit != null)
                        {
                            Listing.lauId = natCodeAndNameUnit.Value.natCode;
                            Listing.lauName = natCodeAndNameUnit.Value.nameUnit;
                        }
                    }
                    break;
                default:
                    {
                        var lauIdAndLauName = Delimitations.LAUParser.GetLauIdAndLauName(Listing);
                        if (lauIdAndLauName != null)
                        {
                            Listing.lauId = lauIdAndLauName.Value.lau_id;
                            Listing.lauName = lauIdAndLauName.Value.lau_name;
                        }
                    }
                    break;
            }
        }

        public static void SetLauIdAndLauNameToListings()
        {
            var listings = ES_Listings.GetListingsWithoutLauName();
            var total = listings.Count;
            var counter = 0;
            var updated = 0;
            var errors = 0;
            Parallel.ForEach(listings, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, listing =>
            {
                Interlocked.Increment(ref counter);
                var lauIdParser = new LauIdParser(CountryCode.ES, listing);
                lauIdParser.SetLauIdAndLauName();
                if (lauIdParser.UpdateLauIdAndLauName())
                {
                    Interlocked.Increment(ref updated);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                int percentage =(int)((double)counter / total * 100);
                Console.WriteLine(counter + "/" + total + " (" + percentage + "%) Updated: " + updated + " Errors: " + errors);
            });
        }

        public bool UpdateLauIdAndLauName()
        {
            string query =
                "UPDATE " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "SET [lauId] = @lauId, [lauName] = @lauName " +
                "WHERE [guid] = @guid";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "lauId", Listing.lauId },
                { "lauName", Listing.lauName },
                { "guid", Listing.guid.ToString() }
            });
        }
    }
}
