using HtmlAgilityPack;
using landerist_library.Parse.Location.Delimitations;
using landerist_library.Websites;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Location
{
    public class LatLngParser(Page page, landerist_orels.ES.Listing listing)
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

            CatastralReferenceToLatLng();
            if (LatLngs.Count > 0)
            {
                SetLatLngToListing();
                return;
            }

            var htmlDocument = Page.GetHtmlDocument();
            if (htmlDocument == null)
            {
                return;
            }
            LatLngIframeGoogleMaps(htmlDocument);
            LatLngInHtmlLatLng(htmlDocument);
            AddressToLatLng();
            SetLatLngToListing();
        }

        private void SetLatLngToListing()
        {
            if (LatLngs.Count.Equals(0))
            {
                return;
            }
            if (LatLngs.Count.Equals(1))
            {
                var tuple = LatLngs.Single();
                Listing.latitude = tuple.Item1;
                Listing.longitude = tuple.Item2;
                Listing.locationIsAccurate = tuple.Item3;
            }
            else
            {
                // todo: decide later
            }
        }

        private void LatLngIframeGoogleMaps(HtmlDocument htmlDocument)
        {
            var iframes = htmlDocument.DocumentNode.Descendants("iframe");
            if (iframes == null)
            {
                return;
            }
            foreach (var iframe in iframes)
            {
                var src = iframe.GetAttributeValue("src", string.Empty);
                if (string.IsNullOrEmpty(src))
                {
                    continue;
                }
                LatLngIframeGoogleMaps(src);
            }
        }

        public void LatLngIframeGoogleMaps(string src)
        {
            if (!src.Contains("https://www.google.com/maps/embed?pb="))
            {
                return;
            }

            // in this order.
            if (src.Contains("!2d") && src.Contains("!3d"))
            {
                LatLngIframeGoogleMaps(src, "!2d", "!3d", false);
            }
            else if (src.Contains("!1d") && src.Contains("!2d"))
            {
                LatLngIframeGoogleMaps(src, "!1d", "!2d", true);
            }
        }

        private void LatLngIframeGoogleMaps(string src, string key1, string key2, bool inverted)
        {
            try
            {
                var lng = src[(src.IndexOf(key1) + key1.Length)..];
                lng = lng[..lng.IndexOf('!')];

                var lat = src[(src.IndexOf(key2) + key2.Length)..];
                lat = lat[..lat.IndexOf('!')];

                if (inverted)
                {
                    AddLatLng(lat, lng, false);
                }
                else
                {
                    AddLatLng(lng, lat, false);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("LatLngParser LatLngIframeGoogleMaps", src, exception);
            }
        }

        private void LatLngInHtmlLatLng(HtmlDocument htmlDocument)
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
                LatLngRegex(text, regex);
            }
        }

        public void LatLngRegex(string text, string regexPattern)
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
                        break;
                    }
                }
            }
        }


        private void AddLatLng(string latitude, string longitude, bool isAccurate)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                AddLatLng(lat, lng, isAccurate);
            }
        }
        private void AddLatLng(double latitude, double longitude, bool isAccurate)
        {
            if (!CountriesParser.ContainsCountry(Page.Website.CountryCode, latitude, longitude))
            {
                return;
            }
            var tuple = Tuple.Create(latitude, longitude, isAccurate);
            LatLngs.Add(tuple);
        }

        private void AddLatLng(Tuple<double, double>? tuple, bool? isAccurate)
        {
            if (tuple == null)
            {
                return;
            }
            if (!isAccurate.HasValue)
            {
                isAccurate = false;
            }
            AddLatLng(tuple.Item1, tuple.Item2, (bool)isAccurate);
        }


        private void AddressToLatLng()
        {
            if (string.IsNullOrEmpty(Listing.address))
            {
                return;
            }

            if (LatLngs.Count > 0)
            {
                return;
            }

            var (latLng, isAccurate) = GoogleMaps.AddressToLatLng.Parse(Listing.address, Page.Website.CountryCode);
            AddLatLng(latLng, isAccurate);
        }

        public void CatastralReferenceToLatLng()
        {
            if (string.IsNullOrEmpty(Listing.cadastralReference))
            {
                return;
            }
            var tuple = Goolzoom.CadastralRefToLatLng.Parse(Listing.cadastralReference);
            if (tuple == null)
            {
                return;
            }
            AddLatLng(tuple.Item1, tuple.Item2, true);
        }
    }
}