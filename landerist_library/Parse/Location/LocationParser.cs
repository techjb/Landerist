using HtmlAgilityPack;
using landerist_library.Parse.Location.Delimitations;
using landerist_library.Websites;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Location
{
    public class LocationParser(Page page, landerist_orels.ES.Listing listing)
    {
        private readonly Page Page = page;

        private readonly landerist_orels.ES.Listing Listing = listing;

        private readonly HashSet<Tuple<double, double, bool>> LatLngs = [];

        public void SetLatLng()
        {
            if (Listing.latitude != null && Listing.longitude != null)
            {
                return;
            }

            FindLatLng();
            SetLatLngToListing();
            SetCadastralReferenceToListing();
        }

        private void FindLatLng()
        {
            if (AddCatastralReferenceLatLng())
            {
                return;
            }

            if (AddAddressLatLng())
            {
                return;
            }
            var htmlDocument = Page.GetHtmlDocument();
            if (htmlDocument == null)
            {
                return;
            }
            if (AddframeGoogleMapsLatLng(htmlDocument))
            {
                return;
            }
            AddCoordinatesInHtmlLatLng(htmlDocument);
        }

        private void SetLatLngToListing()
        {
            if (LatLngs.Count.Equals(0))
            {
                return;
            }
            var tuple = LatLngs.FirstOrDefault();
            if (tuple != null)
            {
                Listing.latitude = tuple.Item1;
                Listing.longitude = tuple.Item2;
                Listing.locationIsAccurate = tuple.Item3;
            }
        }

        private bool AddframeGoogleMapsLatLng(HtmlDocument htmlDocument)
        {
            var iframes = htmlDocument.DocumentNode.Descendants("iframe");
            if (iframes == null)
            {
                return false;
            }
            foreach (var iframe in iframes)
            {
                var src = iframe.GetAttributeValue("src", string.Empty);
                if (string.IsNullOrEmpty(src))
                {
                    continue;
                }
                if (LatLngIframeGoogleMaps(src))
                {
                    return true;
                }
            }
            return false;
        }

        public bool LatLngIframeGoogleMaps(string src)
        {
            if (!src.Contains("https://www.google.com/maps/embed?pb="))
            {
                return false;
            }

            // in this order.
            if (src.Contains("!2d") && src.Contains("!3d"))
            {
                return LatLngIframeGoogleMaps(src, "!2d", "!3d", false);
            }
            if (src.Contains("!1d") && src.Contains("!2d"))
            {
                return LatLngIframeGoogleMaps(src, "!1d", "!2d", true);
            }
            return false;
        }

        private bool LatLngIframeGoogleMaps(string src, string key1, string key2, bool inverted)
        {
            try
            {
                var lng = src[(src.IndexOf(key1) + key1.Length)..];
                lng = lng[..lng.IndexOf('!')];

                var lat = src[(src.IndexOf(key2) + key2.Length)..];
                lat = lat[..lat.IndexOf('!')];

                if (inverted)
                {
                    return AddLatLng(lat, lng, false);
                }
                else
                {
                    return AddLatLng(lng, lat, false);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LatLngParser AddframeGoogleMapsLatLng", src, exception);
            }
            return false;
        }

        private bool AddCoordinatesInHtmlLatLng(HtmlDocument htmlDocument)
        {
            List<string> listRegex =
            [
                @"(latitude|lat|latitud)\s*(=|:)\s*(-?\d+(\.\d+)?).*(longitude|lng|longitud)\s*(=|:)\s*(-?\d+(\.\d+)?)",
                @"LatLng\s*\(\s*(-?\d+\.\d+)\s*(,|\s*)\s*(-?\d+\.\d+)\s*\)"
            ];

            string text = htmlDocument.DocumentNode.InnerHtml
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            foreach (var regex in listRegex)
            {
                if (LatLngRegex(text, regex))
                {
                    return true;
                }
            }
            return false;
        }

        public bool LatLngRegex(string text, string regexPattern)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(text);
            foreach (Match match in matches.Cast<Match>())
            {
                double? latitude = null;
                double? longitude = null;

                foreach (Group group in match.Groups.Cast<Group>())
                {
                    var value = group.Value;
                    if (value.StartsWith('.'))
                    {
                        continue;
                    }
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double latOrLng))
                    {
                        continue;
                    }
                    if (latitude == null)
                    {
                        latitude = latOrLng;
                    }
                    else
                    {
                        longitude = latOrLng;
                        AddLatLng((double)latitude, (double)longitude, false);
                        return true;
                    }
                }
            }
            return false;
        }


        private bool AddLatLng(string latitude, string longitude, bool isAccurate)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                return AddLatLng(lat, lng, isAccurate);
            }
            return false;
        }

        private bool AddLatLng(Tuple<double, double>? tuple, bool? isAccurate)
        {
            if (tuple == null)
            {
                return false;
            }
            if (!isAccurate.HasValue)
            {
                isAccurate = false;
            }
            return AddLatLng(tuple.Item1, tuple.Item2, (bool)isAccurate);
        }
        private bool AddLatLng(double latitude, double longitude, bool isAccurate)
        {
            if (!CountriesParser.ContainsCountry(Page.Website.CountryCode, latitude, longitude))
            {
                return false;
            }
            var tuple = Tuple.Create(latitude, longitude, isAccurate);
            LatLngs.Add(tuple);
            return true;
        }


        private bool AddAddressLatLng()
        {
            if (string.IsNullOrEmpty(Listing.address))
            {
                return false;
            }
            var latLng = new GoogleMaps.GoogleMapsApi().GetLatLng(Listing.address, Page.Website.CountryCode);
            if (latLng == null)
            {
                return false;
            }
            return AddLatLng(latLng.Value.latLng, latLng.Value.isAccurate);

        }

        public bool AddCatastralReferenceLatLng()
        {
            if (string.IsNullOrEmpty(Listing.cadastralReference))
            {
                return false;
            }
            var result = new Goolzoom.GoolzoomApi().GetLatLng(Listing.cadastralReference);
            if (result == null || !result.Value.requestSucess)
            {
                return false;
            }
            if (result.Value.lat == null || result.Value.lng == null)
            {
                Listing.cadastralReference = null;
                return false;
            }
            if (string.IsNullOrEmpty(Listing.address))
            {
                var address = new Goolzoom.GoolzoomApi().GetAddrees(Listing.cadastralReference);
                if (!string.IsNullOrEmpty(address))
                {
                    Listing.address = address;
                }
            }
            return AddLatLng((double)result.Value.lat, (double)result.Value.lng, true);
        }

        private void SetCadastralReferenceToListing()
        {
            if (!string.IsNullOrEmpty(Listing.cadastralReference) || Listing.locationIsAccurate != true)
            {
                return;
            }
            // todo: set cadastral reference doing manual check
        }
    }
}