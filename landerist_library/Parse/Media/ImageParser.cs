using HtmlAgilityPack;
using landerist_library.Database;
using landerist_orels.ES;
using SkiaSharp;
using System.Text.RegularExpressions;


namespace landerist_library.Parse.Media
{
    public class ImageParser
    {
        private readonly MediaParser MediaParser;

        private const int MIN_IMAGE_SIZE = 256 * 256;


        private static readonly HashSet<string> ProhibitedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "icon",
            "logo",
        };

        public ImageParser(MediaParser mediaParser)
        {
            MediaParser = mediaParser;
        }

        public void AddImages()
        {
            AddImagesOpenGraph();
            AddImagesImgSrc();
            //GetImagesA(); // Add some invalid images
        }

        private void AddImagesOpenGraph()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image']");
            AddImages(imageNodes, "content");

            imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image:secure_url']");
            AddImages(imageNodes, "content");
        }

        private void AddImagesImgSrc()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//img");
            AddImages(imageNodes, "src");
        }

        private void AddImagesA()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//a");
            AddImages(imageNodes, "href");
        }

        private void AddImages(HtmlNodeCollection? nodeCollection, string attributeValue)
        {
            if (nodeCollection == null)
            {
                return;
            }
            foreach (var node in nodeCollection)
            {
                AddImage(node, attributeValue);
            }
        }

        private void AddImage(HtmlNode imgNode, string name)
        {
            string attributeValue = imgNode.GetAttributeValue(name, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                return;
            }

            string extension = Path.GetExtension(attributeValue).ToLower();
            if (!extension.StartsWith(".jpg") && !extension.StartsWith(".jpeg"))
            {
                return;
            }

            string title = imgNode.GetAttributeValue("alt", null);
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", null);
            }

            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return;
            }

            if (!IsValidImage(uri))
            {
                return;
            }

            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };

            MediaParser.Media.Add(media);
        }

        private static bool IsValidImage(Uri uri)
        {
            string? filename = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (string.IsNullOrEmpty(filename))
            {
                return true;
            }
            if (FileNameContainsProhibitedWord(filename))
            {
                return false;
            }
            if (FileNameContainsSmallSize(filename))
            {
                return false;
            }
            if (DownloadedImageIsInvalid(uri))
            {
                return false;
            }
            return true;
        }

        private static bool FileNameContainsProhibitedWord(string fileName)
        {
            string filenameParsed = fileName.Replace("_", "-");
            var splitted = filenameParsed.Split('-');
            foreach (var word in splitted)
            {
                foreach (string prohibitedWord in ProhibitedWords)
                {
                    if (word.Equals(prohibitedWord, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FileNameContainsSmallSize(string fileName)
        {
            Regex regex = new(@"(\d+)x(\d+)");
            Match match = regex.Match(fileName);

            if (!match.Success)
            {
                return false;
            }
            if (!match.Groups.Count.Equals(3))
            {
                return false;
            }

            string widthString = match.Groups[1].Value;
            string heightString = match.Groups[2].Value;

            if (!int.TryParse(widthString, out int width) ||
                !int.TryParse(heightString, out int height)
                )
            {
                return false;
            }

            return ImageIsTooSmall(width, height);
        }

        private static bool ImageIsTooSmall(int width, int height)
        {
            int size = width * height;
            return size < MIN_IMAGE_SIZE;
        }

        private static bool DownloadedImageIsInvalid(Uri uri)
        {
            if (DiscardedImages.Contains(uri))
            {
                return true;
            }

            bool isInValid = true;
            try
            {
                using var client = new HttpClient();
                var response = client.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                var responseContent = response.Content;
                Stream stream = responseContent.ReadAsStreamAsync().Result;
                using var sKManagedStream = new SKManagedStream(stream);
                SKBitmap bitmap = SKBitmap.Decode(sKManagedStream);
                SKImage image = SKImage.FromBitmap(bitmap);
                isInValid = ImageIsTooSmall(image.Width, image.Height);
            }
            catch
            {

            }
            if (isInValid)
            {
                DiscardedImages.Insert(uri);
            }
            return isInValid;
        }
    }
}
