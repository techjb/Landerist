namespace landerist_library.Pages
{
    public static class PageNextUpdateCalculator
    {
        private enum TransientErrorKind
        {
            FastRetry,
            Forbidden,
            Gone,
            NotFound,
            ClientError,
            Default,
        }

        public static DateTime? Calculate(Page page, DateTime now)
        {
            if (page.PageType is not PageType pageType || page.PageTypeCounter == null)
            {
                return null;
            }

            double addDays = GetBaseDaysForPageType(pageType);

            if (IsTransientErrorPageType(pageType))
            {
                addDays = GetTransientErrorBackoffDays(page);
            }
            else if (IsLowValuePageType(pageType))
            {
                addDays = GetLowValuePageTypeDays(pageType);
            }
            else if (page.ResponseBodyTextNotChanged)
            {
                addDays = addDays * GetStabilityMultiplier(page.ResponseBodyTextNotChangedCounter);
            }

            if (page.IsListingStatusPublished())
            {
                addDays = Math.Min(addDays, 3d);
            }
            else if (page.IsListingStatusUnPublished())
            {
                addDays = Math.Max(addDays, 45d);
            }

            addDays += GetDeterministicJitterDays(page);
            addDays = Math.Max(1d, addDays);
            return now.AddDays(addDays);
        }

        private static double GetBaseDaysForPageType(PageType pageType)
        {
            return pageType switch
            {
                PageType.MainPage => 3d,
                PageType.MayBeListing => 3d,
                PageType.Listing => 3d,
                PageType.NotListingByParser => 21d,
                PageType.ResponseBodyIsError => 7d,
                PageType.ResponseBodyTooShort => 7d,
                _ => 7d,
            };
        }

        private static bool IsTransientErrorPageType(PageType pageType)
        {
            return pageType == PageType.HttpStatusCodeNotOK ||
                pageType == PageType.Timeout ||
                pageType == PageType.ResponseBodyNullOrEmpty;
        }

        private static bool IsLowValuePageType(PageType pageType)
        {
            return pageType == PageType.IncorrectLanguage ||
                pageType == PageType.BlockedByRobotsTxt ||
                pageType == PageType.CrawlDelayTooBig ||
                pageType == PageType.NotIndexable ||
                pageType == PageType.NotCanonical ||
                pageType == PageType.RedirectToAnotherUrl ||
                pageType == PageType.ResponseBodyTooLarge ||
                pageType == PageType.ResponseBodyRepeatedInHost ||
                pageType == PageType.ResponseBodyTooManyTokens;
        }

        private static double GetTransientErrorBackoffDays(Page page)
        {
            int attempts = Math.Max(1, (int)(page.TransientErrorCounter ?? 1));
            return GetTransientErrorKind(page) switch
            {
                TransientErrorKind.FastRetry => attempts switch
                {
                    1 => 1d,
                    2 => 2d,
                    3 => 4d,
                    4 => 7d,
                    5 => 14d,
                    _ => 30d,
                },
                TransientErrorKind.Forbidden => attempts switch
                {
                    1 => 2d,
                    2 => 4d,
                    3 => 7d,
                    4 => 14d,
                    _ => 30d,
                },
                TransientErrorKind.Gone => attempts switch
                {
                    1 => 60d,
                    2 => 90d,
                    _ => 120d,
                },
                TransientErrorKind.NotFound => attempts switch
                {
                    1 => 7d,
                    2 => 14d,
                    3 => 30d,
                    4 => 60d,
                    _ => 90d,
                },
                TransientErrorKind.ClientError => attempts switch
                {
                    1 => 14d,
                    2 => 30d,
                    3 => 60d,
                    _ => 90d,
                },
                _ => attempts switch
                {
                    1 => 2d,
                    2 => 4d,
                    3 => 7d,
                    4 => 14d,
                    _ => 30d,
                },
            };
        }

        private static TransientErrorKind GetTransientErrorKind(Page page)
        {
            if (page.IsResponseBodyNullOrEmpty() ||
                page.IsHttpStatusCodeTooManyRequests() ||
                page.IsHttpStatusCodeServerError())
            {
                return TransientErrorKind.FastRetry;
            }

            if (page.IsHttpStatusCodeForbidden())
            {
                return TransientErrorKind.Forbidden;
            }

            if (page.IsHttpStatusCodeGone())
            {
                return TransientErrorKind.Gone;
            }

            if (page.IsHttpStatusCodeNotFound())
            {
                return TransientErrorKind.NotFound;
            }

            if (page.IsHttpStatusCodeClientError())
            {
                return TransientErrorKind.ClientError;
            }

            return TransientErrorKind.Default;
        }

        private static double GetLowValuePageTypeDays(PageType pageType)
        {
            return pageType switch
            {
                PageType.BlockedByRobotsTxt => 90d,
                PageType.CrawlDelayTooBig => 60d,
                PageType.IncorrectLanguage => 60d,
                PageType.ResponseBodyRepeatedInHost => 60d,
                PageType.ResponseBodyTooLarge => 45d,
                PageType.ResponseBodyTooManyTokens => 45d,
                _ => 30d,
            };
        }

        private static double GetStabilityMultiplier(short? responseBodyTextNotChangedCounter)
        {
            int stableHits = Math.Max(1, (int)(responseBodyTextNotChangedCounter ?? 1));
            return 1d + (Math.Min(Math.Log2(stableHits + 1d), 3d) * 0.5d);
        }

        private static double GetDeterministicJitterDays(Page page)
        {
            if (string.IsNullOrEmpty(page.UriHash))
            {
                return 0d;
            }

            uint accumulator = 2166136261;
            foreach (char c in page.UriHash)
            {
                accumulator ^= c;
                accumulator *= 16777619;
            }

            int jitterBucket = (int)(accumulator % 21) - 10;
            return jitterBucket / 100d;
        }
    }
}
