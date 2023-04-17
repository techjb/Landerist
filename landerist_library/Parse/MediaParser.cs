using landerist_library.Websites;
using landerist_orels.ES;
using HtmlAgilityPack;

namespace landerist_library.Parse
{
    public class MediaParser
    {
        private readonly Page Page;

        public MediaParser(Page page)
        {
            Page = page;
        }

        public void AddMedia(Listing listing)
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument == null)
            {
                return;
            }
            var mediaImages = GetImages(Page.HtmlDocument);
            listing.SetMedia(mediaImages);

        }

        private SortedSet<Media> GetImages(HtmlDocument htmlDocument)
        {
            SortedSet<Media> medias = new(new MediaComparer());
            var imageNodes = htmlDocument.DocumentNode.SelectNodes("//img");
            if (imageNodes != null)
            {
                foreach (var imgNode in imageNodes)
                {
                    var media = GetMedia(imgNode);
                    if (media != null)
                    {
                        medias.Add(media);
                    }
                }
            }

            return medias;
        }

        private Media? GetMedia(HtmlNode imgNode)
        {
            string src = imgNode.GetAttributeValue("src", null);
            if (string.IsNullOrEmpty(src))
            {
                return null;
            }
            if (!Uri.TryCreate(Page.Uri, src, out Uri? uri))
            {
                return null;
            }
            string title = imgNode.GetAttributeValue("title", null);
            
            return new Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };
        }
    }
}
