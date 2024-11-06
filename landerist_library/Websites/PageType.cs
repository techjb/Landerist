namespace landerist_library.Websites
{
    public enum PageType
    {
        DownloadError,
        IncorrectLanguage,
        BlockedByRobotsTxt,
        CrawlDelayTooBig,
        MainPage,
        NotIndexable,
        NotCanonical,
        ResponseBodyIsError,
        ResponseBodyTooShort,
        ResponseBodyTooLarge,
        ResponseBodyRepeatedInHost,
        ResponseBodyRepeatedInListings,
        ResponseBodyTooManyTokens,
        HtmlNotSimilarToListing,
        MayBeListing,
        Listing,
        NotListingByParser,
        ListingButNotParsed,
        NotListingByLastSegment,
    };
}
