using landerist_library.Websites;
using OpenCvSharp;

namespace landerist_library.Parse.Media.Image
{
    public class ImageDownloader(ImageParser imageParser)
    {

        private readonly ImageParser ImageParser = imageParser;
        private readonly object Sync1 = new();
        private readonly object Sync2 = new();

        public void DownloadImages()
        {
            if (ImageParser.UnknowIsValidImages.Count > 2)
            {
                Parallel.ForEach(ImageParser.UnknowIsValidImages,
                    new ParallelOptions()
                    {
                        //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM,
                    },
                    DownloadImage);
            }
            else
            {
                foreach (var image in ImageParser.UnknowIsValidImages)
                {
                    DownloadImage(image);
                }
            }

            ImageParser.ProcessMediaToRemove(true);
        }

        private void DownloadImage(landerist_orels.Media image)
        {
            if (!ImageParser.MediaParser.WebsiteAccess.Robots.IsAllowed(
                    ImageParser.MediaParser.Page.Website,
                    image.url))
            {
                return;
            }

            if (!Download(image.url))
            {
                lock (Sync1)
                {
                    ImageParser.MediaToRemove.Add(image);
                }
            }
        }

        private bool Download(Uri uri)
        {
            try
            {
                Website website = ImageParser.MediaParser.Page.Website;
                using HttpClient httpClient = ImageParser.MediaParser.WebsiteAccess.HttpClients.Create(
                    website.UseProxy,
                    TimeSpan.FromSeconds(website.Rules.HttpClientTimeoutSeconds));
                using var request = WebsiteHttpRequestProfile.From(website).CreateRequest(HttpMethod.Get, uri);
                using var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                using var mat = Cv2.ImDecode(bytes, ImreadModes.Color);

                if (mat.Empty())
                {
                    return false;
                }

                lock (Sync2)
                {
                    // Avoid duplicate-key exceptions when the same URL appears multiple times.
                    ImageParser.DictionaryMats[uri] = mat.Clone();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
