using landerist_library.Application.Parsing;
using landerist_library.Application.Logging;
using landerist_library.Database;
using landerist_library.Websites;
using Newtonsoft.Json;

namespace landerist_library.Infrastructure.Location.Providers.GoogleMaps
{
    public class GoogleMapsApi : IAddressGeocoder
    {
        private const int GeocodeApiMaxAttempts = 3;

        private readonly GoogleMapsLatLngCache latLngCache;
        private readonly string apiKey;
        private readonly HttpClient httpClient;
        private readonly IApplicationLogger? logger;

        public GoogleMapsApi(
            IDatabase database,
            string apiKey,
            IApplicationLogger? logger = null,
            HttpClient? httpClient = null)
        {
            ArgumentNullException.ThrowIfNull(database);
            this.apiKey = apiKey ?? string.Empty;
            this.logger = logger;
            this.httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            latLngCache = new GoogleMapsLatLngCache(new AddressLatLng(database));
        }

        public GoogleMapsLatLngResult? GetLatLng(string address, CountryCode countryCode = CountryCode.ES)
        {
            return GetLatLngLookup(address, countryCode).Coordinates;
        }

        GeocodedLocation? IAddressGeocoder.GetLatLng(string address, CountryCode countryCode)
        {
            GoogleMapsLatLngResult? result = GetLatLng(address, countryCode);
            return result is null
                ? null
                : new GeocodedLocation(result.Value.Latitude, result.Value.Longitude, result.Value.IsAccurate);
        }

        public GoogleMapsLatLngLookupResult GetLatLngLookup(string address, CountryCode countryCode = CountryCode.ES)
        {
            return latLngCache.GetOrAdd(address, countryCode, GetLatLngFromGoogle);
        }

        private GoogleMapsLatLngLookupResult GetLatLngFromGoogle(string normalizedAddress, string region, CountryCode countryCode)
        {
            var url = GetUrl(normalizedAddress, countryCode);
            try
            {
                var geocodeData = GetGeocodeData(url);
                if (geocodeData != null)
                {
                    var results = geocodeData?.results;
                    if (IsOk(geocodeData) && results != null && results.Length > 0)
                    {
                        var usableResults = results.Where(result => IsUsable(result, countryCode));
                        var selectedResult = usableResults.FirstOrDefault(IsAccurate) ?? usableResults.FirstOrDefault();
                        if (selectedResult == null)
                        {
                            return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
                        }
                        var parsedCoordinates = GetLatLng(selectedResult);
                        if (parsedCoordinates.HasValue)
                        {
                            return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.Found, parsedCoordinates);
                        }

                        return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
                    }

                    if (IsOk(geocodeData) || IsZeroResults(geocodeData))
                    {
                        return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.NotFound, null);
                    }

                    LogGoogleMapsStatus(geocodeData);
                }
            }
            catch (Exception exception)
            {
                logger?.WriteError("GoogleMapsApi GetLatLng", exception.ToString());
            }

            return new GoogleMapsLatLngLookupResult(GoogleMapsLatLngLookupStatus.Error, null);
        }

        private GeocodeData? GetGeocodeData(string url)
        {
            for (var attempt = 1; attempt <= GeocodeApiMaxAttempts; attempt++)
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(content))
                {
                    logger?.WriteError(
                        "GoogleMapsApi GetLatLng",
                        $"Google Maps empty response. Attempt {attempt}/{GeocodeApiMaxAttempts}.");
                    return null;
                }

                var geocodeData = JsonConvert.DeserializeObject<GeocodeData>(content);
                if (!IsRetryableGoogleMapsStatus(geocodeData) || attempt == GeocodeApiMaxAttempts)
                {
                    return geocodeData;
                }

                LogGoogleMapsStatus(geocodeData, attempt, retrying: true);
                Thread.Sleep(GetRetryDelay(attempt));
            }

            return null;
        }

        private string GetUrl(string address, CountryCode countryCode) => "https://maps.googleapis.com/maps/api/geocode/json?" +
            "address=" + Uri.EscapeDataString(address) +
            "&region=" + GoogleMapsCountry.GetRegion(countryCode) +
            GetComponents(countryCode) +
            "&key=" + apiKey;

        private static string GetComponents(CountryCode countryCode)
        {
            return countryCode switch
            {
                CountryCode.ES => "&components=country:ES",
                _ => string.Empty,
            };
        }

        private GoogleMapsLatLngResult? GetLatLng(Result result)
        {
            GoogleMapsLatLngResult? resultLatLng = null;
            if (result.geometry?.location?.lat is double latitude && result.geometry.location.lng is double longitude)
            {
                var isAccurate = IsAccurate(result);
                resultLatLng = new GoogleMapsLatLngResult(latitude, longitude, isAccurate);
            }

            return resultLatLng;
        }

        private static bool IsOk(GeocodeData? geocodeData)
        {
            return string.Equals(geocodeData?.status, "OK", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsZeroResults(GeocodeData? geocodeData)
        {
            return string.Equals(geocodeData?.status, "ZERO_RESULTS", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRetryableGoogleMapsStatus(GeocodeData? geocodeData)
        {
            return string.Equals(geocodeData?.status, "OVER_QUERY_LIMIT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(geocodeData?.status, "UNKNOWN_ERROR", StringComparison.OrdinalIgnoreCase);
        }

        private static TimeSpan GetRetryDelay(int attempt)
        {
            return TimeSpan.FromSeconds(attempt * 2);
        }

        private void LogGoogleMapsStatus(GeocodeData? geocodeData, int? attempt = null, bool retrying = false)
        {
            var status = string.IsNullOrWhiteSpace(geocodeData?.status) ? "NO_STATUS" : geocodeData.status;
            var errorMessage = string.IsNullOrWhiteSpace(geocodeData?.error_message)
                ? "No error_message."
                : geocodeData.error_message;
            var attemptText = attempt.HasValue ? $" Attempt {attempt.Value}/{GeocodeApiMaxAttempts}." : string.Empty;
            var retryText = retrying ? " Retrying." : string.Empty;

            logger?.WriteError(
                "GoogleMapsApi GetLatLng",
                $"Google Maps status: {status}.{attemptText}{retryText} {errorMessage}");
        }

        private static bool IsUsable(Result result, CountryCode countryCode)
        {
            return result != null &&
                   result.partial_match != true &&
                   result.geometry?.location?.lat != null &&
                   result.geometry.location.lng != null &&
                   IsExpectedCountry(result, countryCode);
        }

        private static bool IsExpectedCountry(Result result, CountryCode countryCode)
        {
            var expectedCountryCode = countryCode switch
            {
                CountryCode.ES => "ES",
                _ => null,
            };

            if (expectedCountryCode == null)
            {
                return true;
            }

            return result.address_components?.Any(component =>
                component.types?.Contains("country") == true &&
                string.Equals(component.short_name, expectedCountryCode, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static bool IsAccurate(Result result)
        {
            if (result == null || result.types == null || result.geometry?.location_type == null)
            {
                return false;
            }

            return result.geometry.location_type.Equals("ROOFTOP") &&
                   (result.types.Contains("street_address") ||
                    result.types.Contains("premise") ||
                    result.types.Contains("subpremise"));
        }
    }
}

