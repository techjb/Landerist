using landerist_library.Pages;
using landerist_library.Application.Websites;

namespace landerist_library.Infrastructure.Indexing
{
    public class CanonicalIndexer(Page page, IWebsiteRobotsPolicy robots) : Indexer(page, robots)
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
