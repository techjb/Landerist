using System.Diagnostics.CodeAnalysis;

namespace landerist_library.Websites
{
    public class PageComparer : IEqualityComparer<Page>
    {
        public bool Equals(Page? x, Page? y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.UriHash.Equals(y.UriHash);
        }

        public int GetHashCode([DisallowNull] Page obj)
        {
            return obj.Uri.GetHashCode();
        }
    }
}
