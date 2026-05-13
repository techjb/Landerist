using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Parse.Location.Providers.GoogleMaps
{
    internal class GoogleMapsLatLngCache
    {
        private const int AddressLookupLockCount = 1024;

        private static readonly object[] addressLookupLocks =
            [.. Enumerable.Range(0, AddressLookupLockCount).Select(_ => new object())];

        public GoogleMapsLatLngLookupResult GetOrAdd(
            string address,
            CountryCode countryCode,
            Func<string, string, CountryCode, GoogleMapsLatLngLookupResult> lookup)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
            }

            var normalizedAddress = address.Trim();
            var region = GoogleMapsCountry.GetRegion(countryCode);
            var addressLookupLock = GetAddressLookupLock(normalizedAddress, region);

            lock (addressLookupLock)
            {
                if (AddressLatLng.Select(normalizedAddress, region) is (double lat, double lng, bool isAccurate))
                {
                    return new GoogleMapsLatLngLookupResult(
                        GoogleMapsLatLngLookupStatus.Found,
                        new GoogleMapsLatLngResult(lat, lng, isAccurate));
                }

                var result = lookup(normalizedAddress, region, countryCode);
                if (result.Status == GoogleMapsLatLngLookupStatus.Found && result.Coordinates.HasValue)
                {
                    AddressLatLng.Insert(
                        normalizedAddress,
                        region,
                        result.Coordinates.Value.Latitude,
                        result.Coordinates.Value.Longitude,
                        result.Coordinates.Value.IsAccurate);
                }

                return result;
            }
        }

        private static object GetAddressLookupLock(string normalizedAddress, string region)
        {
            var hashCode = HashCode.Combine(normalizedAddress, region);
            var lockIndex = (hashCode & 0x7fffffff) % AddressLookupLockCount;
            return addressLookupLocks[lockIndex];
        }
    }
}

