using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Websites;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace landerist_library.Parse.Location.GoogleMaps
{
    public class GoogleMapsApi
    {
        public (Tuple<double, double> latLng, bool isAccurate)? GetLatLng(string address, CountryCode countryCode = CountryCode.ES)
        {
            var region = GetRegion(countryCode);
            if (AddressLatLng.Select(address, region) is (double lat, double lng, bool isAccurate)
                //&& Config.IsConfigurationProduction()
                )
            {
                return (Tuple.Create(lat, lng), isAccurate);
            }

            var url = GetUrl(address, countryCode);
            try
            {
                using var httpClient = new HttpClient();
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(content))
                {
                    var geocodeData = JsonConvert.DeserializeObject<GeocodeData>(content);
                    var results = geocodeData?.results;
                    if (results != null && results.Length.Equals(1)) // discarded multiple results
                    {
                        var parsedCoordinates = GetLatLng(results[0]);
                        if (parsedCoordinates.HasValue)
                        {
                            lat = parsedCoordinates.Value.latLng.Item1;
                            lng = parsedCoordinates.Value.latLng.Item2;
                            isAccurate = parsedCoordinates.Value.isAccurate;
                            AddressLatLng.Insert(address, region, lat, lng, isAccurate);
                            return parsedCoordinates;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return null;
        }

        private string GetUrl(string address, CountryCode countryCode) => "https://maps.googleapis.com/maps/api/geocode/json?" +
            "address=" + Uri.EscapeDataString(address) +
            "&region=" + GetRegion(countryCode) +
            //"&extra_computations=BUILDING_AND_ENTRANCES" +
            //"&extra_computations=GROUNDS" +
            "&key=" + PrivateConfig.GOOGLE_CLOUD_LANDERIST_API_KEY;

        private (Tuple<double, double> latLng, bool isAccurate)? GetLatLng(Result result)
        {
            (Tuple<double, double> latLng, bool isAccurate)? resultLatLng = null;
            if (result.geometry?.location?.lat is double latitude && result.geometry.location.lng is double loongitude)
            {
                var latLng = Tuple.Create(latitude, loongitude);
                var isAccurate = IsAccurate(result);
                resultLatLng = (latLng, isAccurate);

                //if (isAccurate && result.buildings?[0].building_outlines?[0].display_polygon is DisplayPolygon displayPolygon)
                //{
                //    latLng = GetCentroid(displayPolygon);
                //    if (latLng != null)
                //    {
                //        resultLatLng = (latLng, true);
                //    }
                //}
            }

            return resultLatLng;
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

        private static Tuple<double, double>? GetCentroid(DisplayPolygon displayPolygon)
        {
            if (displayPolygon == null || displayPolygon.coordinates == null)
            {
                return null;
            }
            switch (displayPolygon.type)
            {
                case "Polygon":
                    {
                        var polygon = ToPolygon(displayPolygon.coordinates);
                        return GetCentroid(polygon);
                    }
                case "MultiPolygon":
                    {
                        var multiPolygon = ToMultiPolygon(displayPolygon.coordinates);
                        return GetCentroid(multiPolygon);
                    }
            }
            return null;
        }


        public static double[][][]? ToPolygon(object obj)
        {
            if (obj is JArray jArray)
            {
                return jArray.ToObject<double[][][]>();
            }

            if (obj is List<object> outerList)
            {
                return [.. outerList
                    .Cast<List<object>>()
                    .Select(innerList => innerList
                        .Cast<List<object>>()
                        .Select(innerInnerList => innerInnerList
                            .Select(Convert.ToDouble)
                            .ToArray()
                        ).ToArray()
                    )];
            }
            return null;
        }

        public static double[][][][]? ToMultiPolygon(object obj)
        {
            if (obj is JArray jArray)
            {
                return jArray.ToObject<double[][][][]>();
            }

            if (obj is List<object> outerList)
            {
                return [.. outerList
                    .Cast<List<object>>()
                    .Select(level1 => level1
                        .Cast<List<object>>()
                        .Select(level2 => level2
                            .Cast<List<object>>()
                            .Select(level3 => level3
                                .Select(Convert.ToDouble)
                                .ToArray()
                            ).ToArray()
                        ).ToArray()
                    )];
            }
            return null;
        }


        private static Tuple<double, double>? GetCentroid(double[][][]? coordinates)
        {
            if (coordinates is null)
            {
                return null;
            }
            try
            {
                var geometryFactory = new GeometryFactory();
                var shellCoordinates = coordinates[0]
                    .Select(coord => new Coordinate(coord[0], coord[1]))
                    .ToArray();

                LinearRing shell = geometryFactory.CreateLinearRing(shellCoordinates);
                Polygon polygon = geometryFactory.CreatePolygon(shell);
                Point centroid = polygon.Centroid;
                return Tuple.Create(centroid.Y, centroid.X);
            }
            catch// (Exception excption)
            {

            }
            return null;
        }

        private static Tuple<double, double>? GetCentroid(double[][][][]? coordinates)
        {
            if (coordinates is null)
            {
                return null;
            }
            try
            {
                var geometryFactory = new GeometryFactory();

                Polygon[] polygons = [.. coordinates.Select(polygonCoords =>
                {
                    Coordinate[] exteriorCoords = [.. polygonCoords[0].Select(point => new Coordinate(point[0], point[1]))];
                    LinearRing shell = geometryFactory.CreateLinearRing(exteriorCoords);
                    LinearRing[] holes = [.. polygonCoords.Skip(1)
                        .Select(ringCoords =>
                            geometryFactory.CreateLinearRing(
                                [.. ringCoords.Select(point => new Coordinate(point[0], point[1]))]
                            )
                        )];

                    return geometryFactory.CreatePolygon(shell, holes);
                })];

                MultiPolygon multiPolygon = geometryFactory.CreateMultiPolygon(polygons);

                Point centroid = multiPolygon.Centroid;
                return Tuple.Create(centroid.Y, centroid.X);
            }
            catch// (Exception excption)
            {

            }
            return null;
        }

        private static string GetRegion(CountryCode countryCode)
        {
            return countryCode switch
            {
                CountryCode.ES => "es",
                _ => string.Empty,
            };
        }

        public static void UpdateListingsLocationIsAccurate()
        {
            var listings = ES_Listings.GetListingsLocationIsAccurateNoCadastralReference();
            if (listings == null || listings.Count == 0)
            {
                return;
            }
            int total = listings.Count;
            int processed = 0;
            int latLngFound = 0;
            int latLngNotFound = 0;
            int accurate = 0;
            int notAccurate = 0;
            int errors = 0;
            Parallel.ForEach(listings,
                new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                listing =>
            {
                double? lat = null;
                double? lng = null;
                bool? locationIsAccurate = null;

                if (listing.address != null)
                {
                    var result = new GoogleMapsApi().GetLatLng(listing.address, CountryCode.ES);
                    if (result != null)
                    {
                        lat = result.Value.latLng.Item1;
                        lng = result.Value.latLng.Item2;
                        locationIsAccurate = result.Value.isAccurate;
                    }
                }


                if (!new ES_Listings().UpdateAddress(listing.guid, lat, lng, locationIsAccurate))
                {
                    Interlocked.Increment(ref errors);
                }

                Interlocked.Increment(ref processed);
                if (locationIsAccurate == true)
                {
                    Interlocked.Increment(ref accurate);
                }
                else
                {
                    Interlocked.Increment(ref notAccurate);
                }
                if (lat is not null && lng is not null)
                {
                    Interlocked.Increment(ref latLngFound);
                }
                else
                {
                    Interlocked.Increment(ref latLngNotFound);
                }

                var accuratePercentage = total > 0 ? (double)accurate / total * 100 : 0;
                var notAccuratePercentage = total > 0 ? (double)notAccurate / total * 100 : 0;
                var latLngFoundPercentage = total > 0 ? (double)latLngFound / total * 100 : 0;
                var latLngNotFoundPercentage = total > 0 ? (double)latLngNotFound / total * 100 : 0;

                Console.WriteLine(
                    $"{processed}/{total}. " +
                    $"latLngFound: {latLngFound} ({(int)latLngFoundPercentage}%) " +
                    $"latLngNotFound: {latLngNotFound} ({(int)latLngNotFoundPercentage}%) " +
                    $"Accurate: {accurate} ({(int)accuratePercentage}%) " +
                    $"NotAccurate: {notAccurate} ({(int)notAccuratePercentage}%) " +
                    $"Errors: {errors}) "
                    );
            });
        }
    }
}
