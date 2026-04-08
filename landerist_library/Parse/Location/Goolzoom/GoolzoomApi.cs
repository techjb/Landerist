using landerist_library.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace landerist_library.Parse.Location.Goolzoom
{
    public class GoolzoomCenter
    {
        public double lat;
        public double lng;
    }

    public class GoolzoomApi
    {
        private const string BASE_URL = "https://api.goolzoom.com/v1/cadastre/";
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        public (bool requestSucess, double? lat, double? lng)? GetLatLng(string cadastralReference)
        {
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }

            string url = BASE_URL + "cadastralreference/" + cadastralReference + "/center";

            bool requestSucess = false;
            double? lat = null;
            double? lng = null;

            try
            {
                var response = HttpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;
                requestSucess = true;

                if (!string.IsNullOrEmpty(content))
                {
                    var goolzoomCenter = JsonConvert.DeserializeObject<GoolzoomCenter>(content);
                    if (goolzoomCenter != null)
                    {
                        lng = goolzoomCenter.lng;
                        lat = goolzoomCenter.lat;
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("GoolzoomApi GetLatLng", exception);
            }

            return (requestSucess, lat, lng);
        }

        public string? GetAddrees(string cadastralReference)
        {
            if (string.IsNullOrEmpty(cadastralReference))
            {
                return null;
            }

            bool isCadastraReference = cadastralReference.Length.Equals(20);
            string url =
                BASE_URL +
                (isCadastraReference ? "cadastralreference/" : "cadastralparcel/") +
                cadastralReference + "/?language=es";

            try
            {
                var response = HttpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(content))
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(content);
                    return GetAddress(data);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("GoolzoomApi GetAddrees", exception);
            }

            return null;
        }

        private static string? GetAddress(dynamic data)
        {
            if (data == null)
            {
                return null;
            }

            dynamic? registros = null;
            if (data.datos != null)
            {
                registros = data.datos[0].registros;
            }
            else if (data.registros != null)
            {
                registros = data.registros;
            }

            if (registros != null)
            {
                List<string?> addressParts =
                [
                    (string?)registros.finca.Dirección,
                    (string?)registros.finca.Municipio,
                    (string?)registros.finca.Provincia
                ];

                return string.Join(", ", addressParts.Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            return null;
        }

        public string? GetAddresses(double latitude, double longitude, int radio)
        {
            string url =
                BASE_URL + "radio/" +
                latitude.ToString(CultureInfo.InvariantCulture) + "/" +
                longitude.ToString(CultureInfo.InvariantCulture) + "/" +
                radio + "/addresses";

            try
            {
                var response = HttpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in GoolzoomApi GetAddresses: {exception.Message} {url}");
                Logs.Log.WriteError("GoolzoomApi GetAddresses", exception);
            }

            return null;
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", Configuration.PrivateConfig.GOOLZOOM_API);
            return client;
        }

        private HttpClient HttpClient => SharedHttpClient;

        public static void UpdateLocationFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReference();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;

            var api = new GoolzoomApi();

            foreach (var listing in listings)
            {
                processed++;

                var latLng = api.GetLatLng(listing.cadastralReference);
                if (latLng is { } result &&
                    result.requestSucess &&
                    result.lat.HasValue &&
                    result.lng.HasValue)
                {
                    listing.latitude = result.lat.Value;
                    listing.longitude = result.lng.Value;
                    listing.locationIsAccurate = true;

                    if (ES_Listings.Update(listing))
                    {
                        updated++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    errors++;
                }

                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }

        public static void UpdateAddressFromCadastralRef()
        {
            var listings = ES_Listings.GetListingWithCatastralReferenceAndNoAddress();
            int total = listings.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;

            var api = new GoolzoomApi();

            foreach (var listing in listings)
            {
                processed++;

                var address = api.GetAddrees(listing.cadastralReference);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    listing.address = address;

                    if (ES_Listings.Update(listing))
                    {
                        updated++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    errors++;
                }

                Console.WriteLine($"Processed {processed}/{total}, Updated: {updated}, Errors: {errors}");
            }
        }
    }
}
