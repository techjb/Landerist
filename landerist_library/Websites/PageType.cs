namespace landerist_library.Websites
{
    public enum PageType
    {
        HttpStatusCodeNotOK,        
        IncorrectLanguage,
        BlockedByRobotsTxt,
        CrawlDelayTooBig,
        MainPage,
        NotIndexable,
        NotCanonical,
        ResponseBodyNullOrEmpty,
        ResponseBodyIsError,
        ResponseBodyTooShort,
        ResponseBodyTooLarge,
        ResponseBodyRepeatedInHost,
        ResponseBodyTooManyTokens,        
        MayBeListing,
        Listing,
        NotListingByParser,
    };

    public enum WaitingStatus
    {
        waiting_ai_request,
        waiting_ai_response,
    }
}
