using HtmlAgilityPack;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.LocationParser
{
    public class LocationParser
    {
        private readonly Page Page;

        private readonly Listing Listing;

        private readonly HashSet<Tuple<double, double>> Locations = new();


        public LocationParser(Page page, Listing listing)
        {

            Page = page;
            Listing = listing;
        }

        public void SetLocation()
        {
            Page.LoadHtmlDocument(true);
            if (Page.HtmlDocument == null)
            {
                return;
            }
            LocationIframeGoogleMaps(Page.HtmlDocument);
            LocationInHtmlLatLng(Page.HtmlDocument);
            SetLocationToListing();
        }

        private void SetLocationToListing()
        {
            if (Locations.Count == 0)
            {
                return;
            }
            if (Locations.Count.Equals(1))
            {
                var tuple = Locations.Single();
                Listing.latitude = tuple.Item1;
                Listing.longitude = tuple.Item2;
            }
            else
            {
                // todo: decide later
            }
        }

        private void LocationIframeGoogleMaps(HtmlDocument htmlDocument)
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
                GetLocationIframeGoogleMaps(src);
            }
        }

        private void GetLocationIframeGoogleMaps(string src)
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

                AddLocation(lat, lng);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }
        }

        private void LocationInHtmlLatLng(HtmlDocument htmlDocument)
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
                GetLocationRegex(text, regex);
            }
        }

        public void GetLocationRegex(string text, string regexPattern)
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
                        AddLocation((double)latitude, (double)longitude);
                        break;
                    }
                }
            }
        }

        private void AddLocation(string latitude, string longitude)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                AddLocation(lat, lng);
            }
        }
        private void AddLocation(double latitude, double longitude)
        {
            var tuple = Tuple.Create(latitude, longitude);
            Locations.Add(tuple);
        }
    }
}
