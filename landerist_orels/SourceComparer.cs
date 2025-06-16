using System;
using System.Collections.Generic;

namespace landerist_orels
{
    public class SourceComparer : IComparer<Source>
    {
        public int Compare(Source x, Source y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x.sourceUrl.AbsoluteUri, y.sourceUrl.AbsoluteUri, StringComparison.OrdinalIgnoreCase);
        }
    }
}
