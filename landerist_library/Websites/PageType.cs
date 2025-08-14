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
        RedirectToAnotherUrl,
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
        readed_by_batch,
        readed_by_localai,
        waiting_ai_response,
    }
}
