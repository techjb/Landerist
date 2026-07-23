using HtmlAgilityPack;
using landerist_library.Pages;
using landerist_library.Parse.CadastralReference;
using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Extractors;
using landerist_library.Parse.Location.Resolvers;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Parse.Location.Validation;

namespace landerist_library.Parse.Location
{
    public class LocationParser
    {
        private readonly Page Page;
        private readonly landerist_orels.ES.Listing Listing;
        private readonly CountryCoordinateValidator CoordinateValidator;
        private readonly HtmlLocationExtractor HtmlLocationExtractor;
        private readonly GoogleMapsAddressLocationResolver GoogleMapsAddressLocationResolver;
        private readonly CadastralLocationResolver CadastralLocationResolver;
        private readonly AddressCadastralReferenceResolver AddressCadastralReferenceResolver;

        public LocationParser(
            Page page,
            landerist_orels.ES.Listing listing,
            GoogleMapsApi googleMapsApi,
            AddressToCadastralReference cadastralReference)
        {
            Page = page;
            Listing = listing;

            CoordinateValidator = new CountryCoordinateValidator(page.Website.CountryCode);
            HtmlLocationExtractor = new HtmlLocationExtractor(CoordinateValidator);
            GoogleMapsAddressLocationResolver = new GoogleMapsAddressLocationResolver(
                page.Website.CountryCode,
                CoordinateValidator,
                googleMapsApi);
            CadastralLocationResolver = new CadastralLocationResolver(CoordinateValidator);
            AddressCadastralReferenceResolver = new AddressCadastralReferenceResolver(cadastralReference);
        }

        public void SetLocation()
        {
            EnsureLatLng();
            SetCadastralReferenceFromAddress();
        }

        private void EnsureLatLng()
        {
            if (Listing.latitude.HasValue || Listing.longitude.HasValue)
            {
                if (ExistingLatLngIsValid())
                {
                    return;
                }

                ClearLatLng();
            }

            SetLatLngToListing(FindLatLng());
        }

        private bool ExistingLatLngIsValid()
        {
            return Listing.latitude.HasValue &&
                Listing.longitude.HasValue &&
                CoordinateValidator.Contains(Listing.latitude.Value, Listing.longitude.Value);
        }

        private void ClearLatLng()
        {
            Listing.latitude = null;
            Listing.longitude = null;
            Listing.locationIsAccurate = null;
            Listing.locationResolver = null;
        }

        private LocationCandidate? FindLatLng()
        {
            var cadastralReferenceCandidate = GetCadastralReferenceLatLng();
            if (cadastralReferenceCandidate != null)
            {
                return cadastralReferenceCandidate;
            }

            var htmlDocument = Page.GetHtmlDocument();
            if (htmlDocument != null)
            {
                var listingCoordinateRegexCandidate = GetListingCoordinateRegexLatLng(htmlDocument);
                if (listingCoordinateRegexCandidate != null)
                {
                    return listingCoordinateRegexCandidate;
                }

                var htmlCandidate = GetHtmlLatLng(htmlDocument);
                if (htmlCandidate != null)
                {
                    return htmlCandidate;
                }
            }

            return GetAddressLatLng();
        }

        private void SetLatLngToListing(LocationCandidate? candidate)
        {
            if (candidate == null)
            {
                return;
            }

            Listing.latitude = candidate.latitude;
            Listing.longitude = candidate.longitude;
            Listing.locationIsAccurate = candidate.isAccurate;
            Listing.locationResolver = candidate.source;
        }

        private LocationCandidate? GetHtmlLatLng(HtmlDocument htmlDocument)
        {
            return HtmlLocationExtractor.TryExtract(htmlDocument, out var candidate)
                ? candidate
                : null;
        }

        private LocationCandidate? GetListingCoordinateRegexLatLng(HtmlDocument htmlDocument)
        {
            var regexPattern = Page.Website.ListingCoordinateRegex;
            if (string.IsNullOrWhiteSpace(regexPattern))
            {
                return null;
            }

            return HtmlLocationExtractor.TryExtractRegex(
                htmlDocument.DocumentNode.InnerHtml,
                regexPattern,
                LocationCandidateSources.ListingCoordinateRegex,
                out var candidate)
                ? candidate
                : null;
        }

        private LocationCandidate? GetAddressLatLng()
        {
            return GoogleMapsAddressLocationResolver.TryResolve(Listing.address, out var candidate)
                ? candidate
                : null;
        }

        private LocationCandidate? GetCadastralReferenceLatLng()
        {
            if (!CadastralLocationResolver.TryResolve(Listing, out var resolution) ||
                resolution == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(Listing.address) &&
                !string.IsNullOrWhiteSpace(resolution.ResolvedAddress))
            {
                Listing.address = resolution.ResolvedAddress;
            }

            return resolution.Candidate;
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
            return string.IsNullOrWhiteSpace(Listing.cadastralReference) &&
                Listing.locationIsAccurate == true &&
                Listing.latitude.HasValue &&
                Listing.longitude.HasValue &&
                !string.IsNullOrWhiteSpace(Listing.address);
        }
    }
}
