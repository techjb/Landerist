using landerist_library.Database;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.Location
{
    public class LauIdParser(CountryCode countryCode, Listing listing)
    {
        private readonly CountryCode _countryCode = countryCode;
        private readonly Listing _listing = listing;

        public void SetLauIdAndLauName()
        {
            switch (_countryCode)
            {
                // More precise map
                case CountryCode.ES:
                {
                    var natCodeAndNameUnit = Delimitations.CNIGParser.GetNatCodeAndNameUnit(_listing);
                    if (natCodeAndNameUnit != null)
                    {
                        _listing.lauId = natCodeAndNameUnit.Value.natCode;
                        _listing.lauName = natCodeAndNameUnit.Value.nameUnit;
                    }
                    break;
                }

                default:
                {
                    var lauIdAndLauName = Delimitations.LAUParser.GetLauIdAndLauName(_listing);
                    if (lauIdAndLauName != null)
                    {
                        _listing.lauId = lauIdAndLauName.Value.lau_id;
                        _listing.lauName = lauIdAndLauName.Value.lau_name;
                    }
                    break;
                }
            }
        }

        public static void SetLauIdAndLauNameToListings()
        {
            var listings = ES_Listings.GetListingsWithoutLauName();
            var total = listings.Count;

            if (total == 0)
            {
                Console.WriteLine("0/0 (0%) Updated: 0 Errors: 0");
                return;
            }

            var counter = 0;
            var updated = 0;
            var errors = 0;

            Parallel.ForEach(listings, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, listingItem =>
            {
                var current = Interlocked.Increment(ref counter);

                var lauIdParser = new LauIdParser(CountryCode.ES, listingItem);
                lauIdParser.SetLauIdAndLauName();

                if (lauIdParser.UpdateLauIdAndLauName())
                {
                    Interlocked.Increment(ref updated);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }

                var percentage = (int)((double)current / total * 100);
                var updatedSnapshot = Volatile.Read(ref updated);
                var errorsSnapshot = Volatile.Read(ref errors);

                Console.WriteLine($"{current}/{total} ({percentage}%) Updated: {updatedSnapshot} Errors: {errorsSnapshot}");
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
                { "lauId", _listing.lauId },
                { "lauName", _listing.lauName },
                { "guid", _listing.guid.ToString() }
            });
        }
    }
}
