namespace landerist_library.Websites
{
    public enum PageType
    {
        DownloadError,
        IncorrectLanguage,
        BlockedByRobotsTxt,
        RobotsTxtDisallow,
        MainPage,
        NotIndexable,
        NotCanonical,
        ResponseBodyIsError,
        ResponseBodyTooShort,
        ResponseBodyTooLarge,
        ResponseBodyRepeatedInHost,
        ResponseBodyTooManyTokens,
        HtmlNotSimilarToListing,
        MayBeListing,
        Listing,
        UnpublishedListing,
        NotListingByParser,
        ListingButNotParsed,
        NotListingByLastSegment,
    };
}
