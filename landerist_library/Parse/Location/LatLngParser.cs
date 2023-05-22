using HtmlAgilityPack;
using landerist_library.Parse.Location.Delimitations;
using landerist_library.Websites;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Location
{
    public class LatLngParser
    {
        private readonly Page Page;

        private readonly landerist_orels.ES.Listing Listing;

        private readonly HashSet<Tuple<double, double>> LatLngs = new();


        public LatLngParser(Page page, landerist_orels.ES.Listing listing)
        {

            Page = page;
            Listing = listing;
        }

        public void SetLatLng()
        {
            Page.LoadHtmlDocument(true);
            if (Page.HtmlDocument == null)
            {
                return;
            }
            LatLngIframeGoogleMaps(Page.HtmlDocument);
            LatLngInHtmlLatLng(Page.HtmlDocument);
            SetLatLngToListing();
        }

        private void SetLatLngToListing()
        {
            if (LatLngs.Count == 0)
            {
                return;
            }
            if (LatLngs.Count.Equals(1))
            {
                var tuple = LatLngs.Single();
                Listing.latitude = tuple.Item1;
                Listing.longitude = tuple.Item2;
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
                lat = lat[..lat.IndexOf("!")];

                AddLatLng(lat, lng);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }
        }

        private void LatLngInHtmlLatLng(HtmlDocument htmlDocument)
        {
            List<string> listRegex = new()
            {
                @"(latitude|lat|latitud)\s*(=|:)\s*(-?\d+(\.\d+)?).*(longitude|lng|longitud)\s*(=|:)\s*(-?\d+(\.\d+)?)",
                @"LatLng\s*\(\s*(-?\d+\.\d+)\s*(,|\s*)\s*(-?\d+\.\d+)\s*\)"
            };

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
                    if (value.StartsWith("."))
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
                        AddLatLng((double)latitude, (double)longitude);
                        break;
                    }
                }
            }
        }

        private void AddLatLng(string latitude, string longitude)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                AddLatLng(lat, lng);
            }
        }
        private void AddLatLng(double latitude, double longitude)
        {
            if (!CountriesParser.ContainsCountry(Page.Website.CountryCode, latitude, longitude))
            {
                return;
            }
            var tuple = Tuple.Create(latitude, longitude);
            LatLngs.Add(tuple);
        }
    }
}
