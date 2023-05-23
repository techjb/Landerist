﻿using landerist_library.Configuration;
using landerist_library.Websites;
using Newtonsoft.Json;

namespace landerist_library.Parse.Location
{
    public class Geometry
    {
        public Location? location { get; set; }
    }

    public class Location
    {
        public string? lat { get; set; }
        public string? lng { get; set; }
    }

    public class Result
    {
        public Geometry? geometry { get; set; }
    }

    public class GeocodeData
    {
        public Result[]? results { get; set; }
    }
    public class GoogleMapsGeocoding
    {
        public static Tuple<string, string>? Geocode(string address, CountryCode countryCode)
        {
            string uriAdress = Uri.EscapeDataString(address);
            string requestUrl =
                "https://maps.googleapis.com/maps/api/geocode/json?" +
                "address=" + uriAdress +
                "&region=" + GetRegion(countryCode) +
                "&key=" + Config.GOOGLE_MAPS_GEOCODING_API;

            try
            {
                HttpClient client = new();
                var response = client.GetAsync(requestUrl).Result;
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                if (content != null)
                {
                    GeocodeData? geocodeData = JsonConvert.DeserializeObject<GeocodeData>(content);
                    if (geocodeData != null && geocodeData.results != null)
                    {
                        if (geocodeData.results.Count() > 0)
                        {
                            var result = geocodeData.results[0];
                            if (result.geometry != null && 
                                result.geometry.location != null && 
                                result.geometry.location.lat != null && 
                                result.geometry.location.lng != null
                                )
                            {
                                string lat = result.geometry.location.lat;
                                string lng = result.geometry.location.lng;
                                return Tuple.Create(lat, lng);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private static string GetRegion(CountryCode countryCode)
        {
            switch(countryCode)
            {
                case CountryCode.ES: return "es";
                default: return string.Empty;
            }
        }
    }
}