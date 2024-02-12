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
        ResponseBodyIsError,
        ResponseBodyTooLarge,
        ResponseBodyTooShort,
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
