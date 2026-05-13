using landerist_library.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;

namespace landerist_library.Parse.Location.Providers.Goolzoom
{
    public class GoolzoomCenter
    {
        [JsonProperty("lat")]
        public double? Lat { get; set; }

        [JsonProperty("lng")]
        public double? Lng { get; set; }
    }

    public sealed record GoolzoomLatLngResult(bool RequestSuccess, double? Latitude, double? Longitude);

    public class GoolzoomApi
    {
        private const string BaseUrl = "https://api.goolzoom.com/v1/cadastre/";
        private const int MaxRetryAttempts = 3;
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        public GoolzoomLatLngResult? GetLatLng(string cadastralReference)
        {
            return GetLatLngAsync(cadastralReference).GetAwaiter().GetResult();
        }

        public async Task<GoolzoomLatLngResult?> GetLatLngAsync(
            string cadastralReference,
            CancellationToken cancellationToken = default)
        {
            string? normalizedCadastralReference = NormalizeReference(cadastralReference);
            if (normalizedCadastralReference is null)
            {
                return null;
            }

            string url = BaseUrl + "cadastralreference/" + Uri.EscapeDataString(normalizedCadastralReference) + "/center";

            try
            {
                using var response = await GetWithRetryAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new GoolzoomLatLngResult(true, null, null);
                }

                var goolzoomCenter = JsonConvert.DeserializeObject<GoolzoomCenter>(content);
                if (goolzoomCenter?.Lat is not double latitude ||
                    goolzoomCenter.Lng is not double longitude ||
                    !IsValidCoordinate(latitude, longitude))
                {
                    return new GoolzoomLatLngResult(true, null, null);
                }

                return new GoolzoomLatLngResult(true, latitude, longitude);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("GoolzoomApi GetLatLng", exception);
            }

            return new GoolzoomLatLngResult(false, null, null);
        }

        public string? GetAddress(string cadastralReference)
        {
            return GetAddressAsync(cadastralReference).GetAwaiter().GetResult();
        }

        public async Task<string?> GetAddressAsync(
            string cadastralReference,
            CancellationToken cancellationToken = default)
        {
            string? normalizedCadastralReference = NormalizeReference(cadastralReference);
            if (normalizedCadastralReference is null)
            {
                return null;
            }

            bool isCadastralReference = normalizedCadastralReference.Length == 20;
            string url =
                BaseUrl +
                (isCadastralReference ? "cadastralreference/" : "cadastralparcel/") +
                Uri.EscapeDataString(normalizedCadastralReference) +
                "/?language=es";

            try
            {
                using var response = await GetWithRetryAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                var data = JToken.Parse(content);
                return GetAddress(data);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("GoolzoomApi GetAddress", exception);
            }

            return null;
        }

        [Obsolete("Use GetAddress instead.")]
        public string? GetAddrees(string cadastralReference)
        {
            return GetAddress(cadastralReference);
        }

        private static string? GetAddress(JToken? data)
        {
            JToken? registros = data?.SelectToken("datos[0].registros") ?? data?.SelectToken("registros");
            JToken? finca = registros?["finca"];
            if (finca is null)
            {
                return null;
            }

            List<string?> addressParts =
            [
                finca["Dirección"]?.ToString(),
                finca["Municipio"]?.ToString(),
                finca["Provincia"]?.ToString()
            ];

            string address = string.Join(
                ", ",
                addressParts
                    .Select(part => part?.Trim())
                    .Where(part => !string.IsNullOrWhiteSpace(part)));

            return string.IsNullOrWhiteSpace(address) ? null : address;
        }

        public string? GetAddresses(double latitude, double longitude, int radio)
        {
            return GetAddressesAsync(latitude, longitude, radio).GetAwaiter().GetResult();
        }

        public async Task<string?> GetAddressesAsync(
            double latitude,
            double longitude,
            int radio,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidCoordinate(latitude, longitude) || radio <= 0)
            {
                return null;
            }

            string url =
                BaseUrl + "radio/" +
                latitude.ToString(CultureInfo.InvariantCulture) + "/" +
                longitude.ToString(CultureInfo.InvariantCulture) + "/" +
                radio.ToString(CultureInfo.InvariantCulture) + "/addresses";

            try
            {
                using var response = await GetWithRetryAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in GoolzoomApi GetAddresses: {exception.Message}");
                Logs.Log.WriteError("GoolzoomApi GetAddresses", exception);
            }

            return null;
        }

        private static async Task<HttpResponseMessage> GetWithRetryAsync(
            string url,
            CancellationToken cancellationToken)
        {
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    var response = await SharedHttpClient.GetAsync(url, cancellationToken);
                    if (!ShouldRetry(response.StatusCode) || attempt == MaxRetryAttempts)
                    {
                        return response;
                    }

                    response.Dispose();
                    await Task.Delay(GetRetryDelay(attempt), cancellationToken);
                }
                catch (HttpRequestException) when (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(GetRetryDelay(attempt), cancellationToken);
                }
            }

            throw new InvalidOperationException("Goolzoom request retry loop ended without a response.");
        }

        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.TooManyRequests ||
                statusCode == HttpStatusCode.RequestTimeout ||
                (int)statusCode >= 500;
        }

        private static TimeSpan GetRetryDelay(int attempt)
        {
            return TimeSpan.FromSeconds(attempt);
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT)
            };

            if (!string.IsNullOrWhiteSpace(PrivateConfig.GOOLZOOM_API))
            {
                client.DefaultRequestHeaders.Add("x-api-key", PrivateConfig.GOOLZOOM_API);
            }

            return client;
        }

        private static string? NormalizeReference(string cadastralReference)
        {
            return string.IsNullOrWhiteSpace(cadastralReference)
                ? null
                : cadastralReference.Trim();
        }

        private static bool IsValidCoordinate(double latitude, double longitude)
        {
            return double.IsFinite(latitude)
                && double.IsFinite(longitude)
                && latitude is >= -90 and <= 90
                && longitude is >= -180 and <= 180;
        }
    }
}

