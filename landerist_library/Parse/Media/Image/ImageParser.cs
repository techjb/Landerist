using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_orels.ES;
using OpenCvSharp;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Media.Image
{
    /// <summary>
    /// In Linux need to add another package. See:
    /// https://stackoverflow.com/questions/44105973/opencvsharp-unable-to-load-dll-opencvsharpextern
    /// </summary>
    public class ImageParser
    {
        private readonly MediaParser MediaParser;

        private const int MIN_IMAGE_SIZE = 256 * 256;

        private readonly SortedSet<landerist_orels.ES.Media> MediaImages = new(new MediaComparer());

        private readonly List<landerist_orels.ES.Media> MediaToRemove = new();

        public readonly SortedSet<landerist_orels.ES.Media> UnknowIsValidImages = new(new MediaComparer());

        public readonly Dictionary<Uri, Mat> DictionaryMats = new();

        public readonly Dictionary<Uri, Mat> NotDuplicatedMats = new();

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
            //GetImagesA(); // Add some invalid currentMat
            RemoveImages();
            foreach (var image in MediaImages)
            {
                MediaParser.Media.Add(image);
            }
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

            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return;
            }

            if (!IsValidImage(uri))
            {
                return;
            }

            string title = MediaParser.GetTitle(imgNode);

            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };

            MediaImages.Add(media);
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

            return ImageIsSmall(width, height);
        }

        private static bool ImageIsSmall(int width, int height)
        {
            int size = width * height;
            return size < MIN_IMAGE_SIZE;
        }

        private void RemoveImages()
        {
            RemoveInvalidImages();
            LoadUnknowIsValid();
            RemoveSmallImages();
            RemoveDuplicatedImages();
            InsertValidImages();
        }
        private void ProcessMediaToRemove(bool addToDiscarded)
        {
            foreach (var image in MediaToRemove)
            {
                MediaImages.Remove(image);
                UnknowIsValidImages.Remove(image);
                if (addToDiscarded)
                {
                    ValidImages.InsertInvalid(image.url);
                }
            }
            MediaToRemove.Clear();
        }

        private void RemoveInvalidImages()
        {
            foreach (var image in MediaImages)
            {
                if (ValidImages.IsInvalid(image.url))
                {
                    MediaToRemove.Add(image);
                }
            }
            ProcessMediaToRemove(false);
        }

        private void LoadUnknowIsValid()
        {
            foreach (var image in MediaImages)
            {
                if (!ValidImages.IsValid(image.url))
                {
                    UnknowIsValidImages.Add(image);
                }
            }
        }

        private void RemoveSmallImages()
        {
            foreach (var image in UnknowIsValidImages)
            {
                if (ImageIsSmall(image.url))
                {
                    MediaToRemove.Add(image);
                }
            }
            ProcessMediaToRemove(true);
        }

        public bool ImageIsSmall(Uri uri)
        {
            bool imageIsSmall = true;
            try
            {
                var stream = GetStream(uri);
                using MemoryStream memoryStream = new();
                stream.CopyTo(memoryStream);
                Mat mat = Cv2.ImDecode(memoryStream.ToArray(), ImreadModes.Color);
                imageIsSmall = ImageIsSmall(mat.Width, mat.Height);
                if (!imageIsSmall)
                {
                    DictionaryMats.Add(uri, mat);
                }
            }
            catch
            {

            }
            return imageIsSmall;
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

        private void RemoveDuplicatedImages()
        {
            new Duplicates(this).FindDuplicates();
            foreach (var image in UnknowIsValidImages)
            {
                if (!NotDuplicatedMats.ContainsKey(image.url))
                {
                    MediaToRemove.Add(image);
                }
            }
            ProcessMediaToRemove(true);
        }        

        private void InsertValidImages()
        {
            foreach (var image in UnknowIsValidImages)
            {
                ValidImages.InsertValid(image.url);
            }
        }
    }
}
