using landerist_library.Websites;
using landerist_orels.ES;
using System.Globalization;

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
            if (Page.HtmlDocument == null)
            {
                return;
            }
            GetLocationGoogleMaps();
        }

        private void GetLocationGoogleMaps()
        {
            var iframes = Page.HtmlDocument!.DocumentNode.Descendants("iframe");
            foreach (var iframe in iframes)
            {
                var srcAttribute = iframe.GetAttributeValue("src", string.Empty);
                try
                {
                    TryParseGoogleMaps(srcAttribute);
                    if (LocationFound)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void TryParseGoogleMaps(string srcAttribute)
        {
            if (srcAttribute.Contains("https://www.google.com/maps/embed?pb=") &&
                    srcAttribute.Contains("!2d") &&
                    srcAttribute.Contains("!3d")
                    )
            {
                var lng = srcAttribute[(srcAttribute.IndexOf("!2d") + 3)..];
                lng = lng[..lng.IndexOf("!3d")];

                var lat = srcAttribute[(srcAttribute.IndexOf("!3d") + 3)..];
                lat = lat[..lat.IndexOf("!")];

                if (double.TryParse(lng, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) &&
                    double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude)
                    )
                {
                    Listing.longitude = longitude;
                    Listing.latitude = latitude;
                    LocationFound = true;
                }
            }
        }
    }
}
