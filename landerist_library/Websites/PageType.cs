using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        ForbiddenLastSegment,
        ResponseBodyError,
        ResponseBodyTooLarge,
        ResponseBodyTooShort,
        ResponseBodyTooManyTokens,
        MayBeListing,
        Listing,
        NotListing,
    };
}
