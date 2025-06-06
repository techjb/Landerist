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
        MayBeListing,
        Listing,
        NotListingByParser,
        ListingButNotParsed,
    };

    public enum WaitingStatus
    {
        //waiting_for_scrape,
        waiting_ai_request,
        waiting_ai_response,
    }
}
