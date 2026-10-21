using landerist_library.Database;
using landerist_library.Application.Logging;
using System.Text;
using System.Text.RegularExpressions;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Location.Providers.GoogleMaps
{
    internal class GoogleMapsLatLngCache
    {
        private const int AddressLookupLockCount = 1024;

        private static readonly object[] addressLookupLocks =
            [.. Enumerable.Range(0, AddressLookupLockCount).Select(_ => new object())];

        private readonly AddressLatLng _cache;
        private readonly IApplicationLogger? logger;
        private readonly int negativeCacheLifetimeDays;

        public GoogleMapsLatLngCache(AddressLatLng cache, IApplicationLogger? logger, int negativeCacheLifetimeDays)
        {
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache;
            this.logger = logger;
            this.negativeCacheLifetimeDays = negativeCacheLifetimeDays;
        }

        public GoogleMapsLatLngLookupResult GetOrAdd(
            string address,
            CountryCode countryCode,
            Func<string, string, CountryCode, GoogleMapsLatLngLookupResult> lookup)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
            }

            var normalizedAddress = Regex.Replace(address.Normalize(NormalizationForm.FormC).Trim(), @"\s+", " ");
            if (normalizedAddress.Length > 400)
            {
                logger?.WriteError("GoogleMaps cache", "Address exceeds the 400 character cache limit; geocoding skipped.");
                return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.Error, null);
            }
            var region = GoogleMapsCountry.GetRegion(countryCode);
            var addressLookupLock = GetAddressLookupLock(normalizedAddress, region);

            lock (addressLookupLock)
            {
                // Preserve hits for entries written before whitespace/Unicode normalization.
                var cached = _cache.Select(normalizedAddress, region);
                if (cached is null && !string.Equals(normalizedAddress, address.Trim(), StringComparison.Ordinal))
                    cached = _cache.Select(address.Trim(), region);
                if (cached is (double lat, double lng, bool isAccurate))
                {
                    return new GoogleMapsLatLngLookupResult(
                        GoogleMapsLatLngLookupStatus.Found,
                        new GoogleMapsLatLngResult(lat, lng, isAccurate));
                }

                if (_cache.IsNotFound(normalizedAddress, region))
                {
                    return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
                }

                var result = lookup(normalizedAddress, region, countryCode);
                if (result.Status == GoogleMapsLatLngLookupStatus.Found && result.Coordinates.HasValue)
                {
                    TryWrite(() => _cache.Insert(
                        normalizedAddress,
                        region,
                        result.Coordinates.Value.Latitude,
                        result.Coordinates.Value.Longitude,
                        result.Coordinates.Value.IsAccurate));
                }

                else if (result.Status == GoogleMapsLatLngLookupStatus.NotFound)
                {
                    TryWrite(() => _cache.InsertNotFound(normalizedAddress, region, negativeCacheLifetimeDays));
                }

                return result;
            }
        }

        private void TryWrite(Func<bool> write)
        {
            try
            {
                if (!write())
                    logger?.WriteError("GoogleMaps cache", "Cache insertion failed; future lookups may call Google again.");
            }
            catch (Exception exception)
            {
                logger?.WriteError("GoogleMaps cache", $"Cache insertion failed: {exception.GetType().Name}.");
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

