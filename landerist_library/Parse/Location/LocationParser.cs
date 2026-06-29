using HtmlAgilityPack;
using landerist_library.Pages;
using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Extractors;
using landerist_library.Parse.Location.Resolvers;
using landerist_library.Parse.Location.Validation;

namespace landerist_library.Parse.Location
{
    public class LocationParser
    {
        private readonly Page Page;
        private readonly landerist_orels.ES.Listing Listing;
        private readonly HtmlLocationExtractor HtmlLocationExtractor;
        private readonly GoogleMapsAddressLocationResolver GoogleMapsAddressLocationResolver;
        private readonly CadastralLocationResolver CadastralLocationResolver;
        private readonly AddressCadastralReferenceResolver AddressCadastralReferenceResolver;

        public LocationParser(Page page, landerist_orels.ES.Listing listing)
        {
            Page = page;
            Listing = listing;

            var coordinateValidator = new CountryCoordinateValidator(page.Website.CountryCode);
            HtmlLocationExtractor = new HtmlLocationExtractor(coordinateValidator);
            GoogleMapsAddressLocationResolver = new GoogleMapsAddressLocationResolver(
                page.Website.CountryCode,
                coordinateValidator);
            CadastralLocationResolver = new CadastralLocationResolver(coordinateValidator);
            AddressCadastralReferenceResolver = new AddressCadastralReferenceResolver();
        }

        private LocationCandidate? LocationCandidate;

        public void SetLocation()
        {
            EnsureLatLng();
            SetCadastralReferenceFromAddress();
        }

        private void EnsureLatLng()
        {
            if (Listing.latitude != null && Listing.longitude != null)
            {
                return;
            }

            FindLatLng();
            SetLatLngToListing();
        }

        private void FindLatLng()
        {
            if (AddCatastralReferenceLatLng())
            {
                return;
            }

            var htmlDocument = Page.GetHtmlDocument();
            if (htmlDocument != null && AddListingCoordinateRegexLatLng(htmlDocument))
            {
                return;
            }

            if (AddAddressLatLng())
            {
                return;
            }

            if (htmlDocument != null && AddHtmlLatLng(htmlDocument))
            {
                return;
            }
        }

        private void SetLatLngToListing()
        {
            if (LocationCandidate == null)
            {
                return;
            }

            Listing.latitude = LocationCandidate.latitude;
            Listing.longitude = LocationCandidate.longitude;
            Listing.locationIsAccurate = LocationCandidate.isAccurate;
        }

        private bool AddHtmlLatLng(HtmlDocument htmlDocument)
        {
            return HtmlLocationExtractor.TryExtract(htmlDocument, out var candidate) &&
                AddLocationCandidate(candidate);
        }

        private bool AddListingCoordinateRegexLatLng(HtmlDocument htmlDocument)
        {
            var regexPattern = Page.Website.ListingCoordinateRegex;
            if (string.IsNullOrWhiteSpace(regexPattern))
            {
                return false;
            }

            return HtmlLocationExtractor.TryExtractRegex(
                htmlDocument.DocumentNode.InnerHtml,
                regexPattern,
                LocationCandidateSources.ListingCoordinateRegex,
                out var candidate) &&
                AddLocationCandidate(candidate);
        }

        public bool LatLngIframeGoogleMaps(string src)
        {
            return HtmlLocationExtractor.TryExtractGoogleMapsIframe(src, out var candidate) &&
                AddLocationCandidate(candidate);
        }

        public bool LatLngRegex(
            string text,
            string regexPattern,
            string source = LocationCandidateSources.Html)
        {
            return HtmlLocationExtractor.TryExtractRegex(text, regexPattern, source, out var candidate) &&
                AddLocationCandidate(candidate);
        }

        private bool AddAddressLatLng()
        {
            return GoogleMapsAddressLocationResolver.TryResolve(Listing.address, out var candidate) &&
                AddLocationCandidate(candidate);
        }

        public bool AddCatastralReferenceLatLng()
        {
            return CadastralLocationResolver.TryResolve(Listing, out var candidate) &&
                AddLocationCandidate(candidate);
        }

        private bool AddLocationCandidate(LocationCandidate? candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            LocationCandidate = candidate;
            return true;
        }

        private void SetCadastralReferenceFromAddress()
        {
            if (!CanResolveCadastralReferenceFromAddress())
            {
                return;
            }
            var cadastralReference = AddressCadastralReferenceResolver.Resolve(
                Listing.latitude,
                Listing.longitude,
                Listing.address);
            if (!string.IsNullOrEmpty(cadastralReference))
            {
                Listing.cadastralReference = cadastralReference;
            }
        }

        private bool CanResolveCadastralReferenceFromAddress()
        {
            return string.IsNullOrEmpty(Listing.cadastralReference) &&
                Listing.locationIsAccurate == true &&
                !string.IsNullOrEmpty(Listing.address);
        }
    }
}
