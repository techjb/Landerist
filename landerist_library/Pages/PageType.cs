namespace landerist_library.Pages
{
    public enum PageType
    {
        HttpStatusCodeNull,
        HttpStatusCodeOtherNotOK,
        HttpStatusCodeNotFound,
        HttpStatusCodeGone,
        DiscardedByListingUrlRegex,
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
        NotListingByCache,
        NotListingByWebsiteRule
    };

    public enum WaitingStatus
    {
        waiting_ai_request,
        readed_by_batch,
        readed_by_localai,
        waiting_ai_response,
    }
}
