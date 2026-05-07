using landerist_library.Configuration;

namespace landerist_library.Pages
{
    public partial class Page
    {
        public void SetPageType(PageType? newPageType)
        {
            SetTransientErrorCounter(newPageType);

            if (PageType == newPageType)
            {
                PageTypeCounter = (short)Math.Min((PageTypeCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER);
            }
            else
            {
                PageTypeCounter = 1;
                PageType = newPageType;
            }
        }

        public void SetWaitingStatusAIRequest()
        {
            SetWaitingStatus(landerist_library.Pages.WaitingStatus.waiting_ai_request);
        }

        private void SetWaitingStatus(WaitingStatus waitingStatus)
        {
            WaitingStatus = waitingStatus;
        }

        public void RemoveWaitingStatus()
        {
            WaitingStatus = null;
        }

        public bool IsMayBeListing()
        {
            return PageType == landerist_library.Pages.PageType.MayBeListing;
        }

        public bool IsHttpStatusCodeNotOK()
        {
            return PageType == landerist_library.Pages.PageType.HttpStatusCodeNotOK;
        }

        public bool IsHttpStatusCodeNotFound()
        {
            return HttpStatusCode == 404;
        }

        public bool IsHttpStatusCodeTooManyRequests()
        {
            return HttpStatusCode == 429;
        }

        public bool IsHttpStatusCodeForbidden()
        {
            return HttpStatusCode == 403;
        }

        public bool IsHttpStatusCodeGone()
        {
            return HttpStatusCode == 410;
        }

        public bool IsHttpStatusCodeServerError()
        {
            return HttpStatusCode >= 500 && HttpStatusCode <= 599;
        }

        public bool IsHttpStatusCodeClientError()
        {
            return HttpStatusCode >= 400 && HttpStatusCode <= 499;
        }

        public bool IsResponseBodyNullOrEmpty()
        {
            return PageType == landerist_library.Pages.PageType.ResponseBodyNullOrEmpty;
        }

        public bool IsListing()
        {
            return PageType == landerist_library.Pages.PageType.Listing;
        }

        public bool IsNotListingByParser()
        {
            return PageType == landerist_library.Pages.PageType.NotListingByParser;
        }

        public bool IsNotListingByCache()
        {
            return PageType == landerist_library.Pages.PageType.NotListingByCache;
        }

        public bool IsNotCanonical()
        {
            return PageType == landerist_library.Pages.PageType.NotCanonical;
        }

        public bool IsRedirectToAnotherUrl()
        {
            return PageType == landerist_library.Pages.PageType.RedirectToAnotherUrl;
        }

        public bool IsDiscardedByListingUrlRegex()
        {
            return PageType == landerist_library.Pages.PageType.DiscardedByListingUrlRegex;
        }

        private void SetTransientErrorCounter(PageType? newPageType)
        {
            if (newPageType == landerist_library.Pages.PageType.HttpStatusCodeNotOK ||
                newPageType == landerist_library.Pages.PageType.ResponseBodyNullOrEmpty)
            {
                TransientErrorCounter = (short)Math.Min((TransientErrorCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER);
                return;
            }

            TransientErrorCounter = 0;
        }
    }
}
