using landerist_library.Websites;

namespace landerist_library.Index
{
    public class CanonicalIndexer(Page page) : Indexer(page)
    {
        public void Insert()
        {
            var canonicalUrl = Page.GetCanonicalUri();
            if (canonicalUrl != null)
            {
                Insert(canonicalUrl);
            }
        }
    }
}
