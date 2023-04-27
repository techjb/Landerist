﻿using System.Diagnostics.CodeAnalysis;

namespace landerist_library.Websites
{
    public class PageComparer : IEqualityComparer<Page>
    {
        public bool Equals(Page? x, Page? y)
        {
            return x.Uri.Equals(y.Uri);
        }

        public int GetHashCode([DisallowNull] Page obj)
        {
            return obj.Uri.GetHashCode();
        }
    }
}
