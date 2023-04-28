using HtmlAgilityPack;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse
{
    public class LocationParser
    {
        private readonly Page Page;

        private readonly Listing Listing;

        private bool LocationFound = false;

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
            if (LocationFound)
            {
                return;
            }
            GetLocationIframeGoogleMaps(Page.HtmlDocument);
            if (LocationFound)
            {
                return;
            }
            GetLocationRegex(Page.HtmlDocument);
        }

        private void GetLocationIframeGoogleMaps(HtmlDocument htmlDocument)
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
                if (LocationFound)
                {
                    return;
                }
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

                SetLocation(lat, lng);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }
        }

        private void GetLocationRegex(HtmlDocument htmlDocument)
        {
            List<string> listRegex = new()
            {
                @"(latitude|lat|latitud)\s*(=|:)\s*(-?\d+(\.\d+)?)\s*(,|;)\s*(longitude|lng|longitud)\s*(=|:)\s*(-?\d+(\.\d+)?)",
                @"LatLng\s*\(\s*(-?\d+\.\d+)\s*(,|\s*)\s*(-?\d+\.\d+)\s*\)"
            };
            foreach (var regex in listRegex)
            {
                if (LocationFound)
                {
                    break;
                }
                GetLocationRegex(htmlDocument.DocumentNode.InnerHtml, regex);
            }
        }

        public void GetLocationRegex(string text, string regexPattern)
        {
            var matches = new Regex(regexPattern).Matches(text);
            foreach (Match match in matches.Cast<Match>())
            {
                if (LocationFound)
                {
                    return;
                }

                string? latitude;
                string? longitude;
                switch (match.Groups.Count)
                {
                    case 3: latitude = match.Groups[1].Value; longitude = match.Groups[2].Value; break;
                    case 5: latitude = match.Groups[1].Value; longitude = match.Groups[3].Value; break;
                    default: continue;
                }
                if (latitude != null && longitude != null)
                {
                    SetLocation(latitude, longitude);
                }
            }
        }

        private void SetLocation(string latitude, string longitude)
        {
            if (double.TryParse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lng) &&
                double.TryParse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat))
            {
                SetLocation(lat, lng);
            }
        }
        private void SetLocation(double latitude, double longitude)
        {
            Listing.longitude = longitude;
            Listing.latitude = latitude;
            LocationFound = true;
        }
    }
}
