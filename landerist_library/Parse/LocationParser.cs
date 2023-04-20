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
            catch (Exception ex)
            {

            }
        }

        private void GetLocationRegex(HtmlDocument htmlDocument)
        {
            List<string> listRegex = new()
            {
                @"latitude=(-?\d+(\.\d+)?),\s*longitude=(-?\d+(\.\d+)?)",
                @"lat:\s*(-?\d+\.\d+)\s*,\s*lng:\s*(-?\d+\.\d+)",

            };
            foreach(var regex in listRegex)
            {
                if (LocationFound)
                {
                    break;
                }
                GetLocationRegex(htmlDocument, regex);
            }
        }

        private void GetLocationRegex(HtmlDocument htmlDocument, string regexPattern)
        {
            var regex = new Regex(regexPattern);
            var match = regex.Match(htmlDocument.DocumentNode.InnerHtml);

            if (match.Success)
            {
                string latitude = match.Groups[1].Value;
                string longitude = match.Groups[2].Value;
                SetLocation(latitude, longitude);
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
