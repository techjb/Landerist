using landerist_library.Configuration;
using OpenCvSharp;

namespace landerist_library.Parse.Media.Image
{
    public class ImageDownloader(ImageParser imageParser)
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

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
            if (!ImageParser.MediaParser.Page.Website.IsAllowedByRobotsTxt(image.url))
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
                var bytes = HttpClient.GetByteArrayAsync(uri).GetAwaiter().GetResult();
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

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            return client;
        }
    }
}
