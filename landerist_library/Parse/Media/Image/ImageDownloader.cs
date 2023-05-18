using landerist_library.Configuration;
using OpenCvSharp;

namespace landerist_library.Parse.Media.Image
{
    public class ImageDownloader
    {
        public static bool Download(ImageParser ImageParser, Uri uri)
        {
            try
            {
                var stream = GetStream(uri);
                using MemoryStream memoryStream = new();
                stream.CopyTo(memoryStream);
                Mat mat = Cv2.ImDecode(memoryStream.ToArray(), ImreadModes.Color);
                ImageParser.DictionaryMats.Add(uri, mat);
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
