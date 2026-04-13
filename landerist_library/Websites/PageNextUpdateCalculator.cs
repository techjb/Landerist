using landerist_library.Configuration;

namespace landerist_library.Websites
{
    public static class PageNextUpdateCalculator
    {
        public static DateTime? Calculate(Page page, DateTime now)
        {
            if (page.PageType == null || page.PageTypeCounter == null)
            {
                return null;
            }

            double addDays = GetBaseDaysForPageType(page.PageType.Value);

            if (page.PageType == PageType.HttpStatusCodeNotOK ||
                page.PageType == PageType.ResponseBodyNullOrEmpty)
            {
                addDays = GetErrorBackoffDays(page.PageTypeCounter.Value);
            }
            else if (page.ResponseBodyTextNotChanged)
            {
                addDays = addDays * GetStabilityMultiplier(page.PageTypeCounter.Value);
            }

            if (page.IsListingStatusPublished())
            {
                addDays = Math.Min(addDays, Config.DEFAULT_DAYS_NEXT_UPDATE_LISTING);
            }

            addDays = Math.Clamp(addDays, Config.MIN_DAYS_NEXT_UPDATE, Config.MAX_DAYS_NEXT_UPDATE);
            return now.AddDays(addDays);
        }

        private static double GetBaseDaysForPageType(PageType pageType)
        {
            return pageType switch
            {
                PageType.MainPage => Config.DEFAULT_DAYS_NEXT_UPDATE,
                PageType.MayBeListing => Config.DEFAULT_DAYS_NEXT_UPDATE,
                PageType.Listing => Config.DEFAULT_DAYS_NEXT_UPDATE_LISTING,
                _ => Config.DEFAULT_DAYS_NEXT_UPDATE,
            };
        }

        private static double GetErrorBackoffDays(short pageTypeCounter)
        {
            int attempts = Math.Max(1, (int)pageTypeCounter);
            return Math.Pow(2, attempts - 1);
        }

        private static double GetStabilityMultiplier(short pageTypeCounter)
        {
            int stableHits = Math.Max(1, (int)pageTypeCounter);
            return 1d + Math.Log2(stableHits + 1d);
        }
    }
}
