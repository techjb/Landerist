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

            Page.LoadHtmlDocument(true);
            if (Page.HtmlDocument == null)
            {
                return;
            }
            LatLngIframeGoogleMaps(Page.HtmlDocument);
            LatLngInHtmlLatLng(Page.HtmlDocument);
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

        private void LatLngIframeGoogleMaps(string src)
        {
            if (!src.Contains("https://www.google.com/maps/embed?pb=") &&
                !src.Contains("!2d") &&
                !src.Contains("!3d"))
            {
                return;
            }

            try
            {
                var lng = src[(src.IndexOf("!2d") + 3)..];
                lng = lng[..lng.IndexOf("!3d")];

                var lat = src[(src.IndexOf("!3d") + 3)..];
                lat = lat[..lat.IndexOf('!')];

                AddLatLng(lat, lng, false);
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

        private void AddLatLng(string latitude, string longitude, bool locationIsAccurate)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                AddLatLng(lat, lng, locationIsAccurate);
            }
        }
        private void AddLatLng(double latitude, double longitude, bool locationIsAccurate)
        {
            if (!CountriesParser.ContainsCountry(Page.Website.CountryCode, latitude, longitude))
            {
                return;
            }
            var tuple = Tuple.Create(latitude, longitude, locationIsAccurate);
            LatLngs.Add(tuple);
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

            var tuple = GoogleMaps.AddressToLatLng.Parse(Listing.address, Page.Website.CountryCode);
            if (tuple == null)
            {
                return;
            }

            AddLatLng(tuple.Item1, tuple.Item2, false);
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