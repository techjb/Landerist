using landerist_library.Application.Listings;
using landerist_library.Parse.Location;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LauIdListingsUpdater(IListingAdministrationService listings)
{
    public void Update()
    {
        var items = listings.GetWithoutLauName();
        int total = items.Count;
        int processed = 0;
        int updated = 0;
        int errors = 0;

        Parallel.ForEach(items, listing =>
        {
            var parser = new LauIdParser(CountryCode.ES, listing);
            parser.SetLauIdAndLauName();

            if (listings.Update(listing))
            {
                Interlocked.Increment(ref updated);
            }
            else
            {
                Interlocked.Increment(ref errors);
            }

            int current = Interlocked.Increment(ref processed);
            int percentage = total == 0 ? 0 : (int)((double)current / total * 100);
            Console.WriteLine(
                $"{current}/{total} ({percentage}%) Updated: {updated} Errors: {errors}");
        });
    }
}
