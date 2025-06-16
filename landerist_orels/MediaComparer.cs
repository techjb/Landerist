using System;
using System.Collections.Generic;

namespace landerist_orels
{
    public class MediaComparer : IComparer<Media>
    {
        public int Compare(Media x, Media y)
        {
            if (x.url == null && y.url == null) return 0;
            if (x.url == null) return -1;
            if (y.url == null) return 1;

            return string.Compare(
                x.url.AbsoluteUri.Trim(),
                y.url.AbsoluteUri.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
