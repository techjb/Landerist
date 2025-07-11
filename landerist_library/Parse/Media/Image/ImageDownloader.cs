﻿using landerist_library.Configuration;
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
                var stream = GetStream(uri);
                using MemoryStream memoryStream = new();
                stream.CopyTo(memoryStream);
                Mat mat = Cv2.ImDecode(memoryStream.ToArray(), ImreadModes.Color);
                lock (Sync2)
                {
                    ImageParser.DictionaryMats.Add(uri, mat);
                }
                return true;
            }
            catch
            {

            }
            return false;
        }

        private static Stream GetStream(Uri uri)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            var response = client.GetAsync(uri).Result;
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content;
            return responseContent.ReadAsStreamAsync().Result;
        }
    }
}
