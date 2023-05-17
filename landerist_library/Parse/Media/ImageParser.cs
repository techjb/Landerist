using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_orels.ES;
using OpenCvSharp;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Media
{
    /// <summary>
    /// In Linux need to add another package. See:
    /// https://stackoverflow.com/questions/44105973/opencvsharp-unable-to-load-dll-opencvsharpextern
    /// </summary>
    public class ImageParser
    {
        private readonly MediaParser MediaParser;

        private const int MIN_IMAGE_SIZE = 256 * 256;

        private static readonly SortedSet<landerist_orels.ES.Media> MediaImages = new(new MediaComparer());

        private static readonly List<landerist_orels.ES.Media> MediaToRemove = new();

        private static readonly SortedSet<landerist_orels.ES.Media> UnknowIsValidImages = new(new MediaComparer());

        private static readonly Dictionary<Uri, Mat> DictionaryMats = new();

        private static readonly Dictionary<Uri, Mat> NotDuplicatedMats = new();

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

            string title = MediaParser.GetTitle(imgNode);

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

        private static void RemoveImages()
        {
            RemoveInvalidImages();
            LoadUnknowIsValid();
            RemoveSmallImages();
            RemoveDuplicatedImages();
            InsertValidImages();
        }
        private static void ProcessMediaToRemove(bool addToDiscarded)
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

        private static void RemoveInvalidImages()
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

        private static void LoadUnknowIsValid()
        {
            foreach (var image in MediaImages)
            {
                if (!ValidImages.IsValid(image.url))
                {
                    UnknowIsValidImages.Add(image);
                }
            }
        }

        private static void RemoveSmallImages()
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

        public static bool ImageIsSmall(Uri uri)
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

        private static void RemoveDuplicatedImages()
        {
            GetDuplicates();
            foreach (var image in UnknowIsValidImages)
            {
                if (!NotDuplicatedMats.ContainsKey(image.url))
                {
                    MediaToRemove.Add(image);
                }
            }
            ProcessMediaToRemove(true);
        }

        private static void GetDuplicates()
        {
            foreach (var kvp in DictionaryMats)
            {
                DictionaryMats[kvp.Key] = CalculateHystogram(kvp.Value);
            }
            foreach (var image in UnknowIsValidImages)
            {
                GetDuplicates(image);
            }
        }

        private static void GetDuplicates(landerist_orels.ES.Media image)
        {
            if (!DictionaryMats.TryGetValue(image.url, out Mat? currentMat))
            {
                return;
            }

            var existingImage = NotDuplicatedMats.FirstOrDefault(kvp => AreSimilar(kvp.Value, currentMat));
            if (existingImage.Equals(default(KeyValuePair<Uri, Mat>)))
            {
                NotDuplicatedMats[image.url] = currentMat;
                return;
            }
            if (existingImage.Value.Width * existingImage.Value.Height < currentMat.Width * currentMat.Height)
            {
                NotDuplicatedMats.Remove(existingImage.Key);
                NotDuplicatedMats[image.url] = currentMat;
            }
        }

        private static Mat CalculateHystogram(Mat mat)
        {
            Mat hist = new();
            Cv2.CalcHist(new Mat[] { mat }, new int[] { 0 }, null, hist, 1, 
                new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
            return hist;
        }

        private static bool AreSimilar(Mat mat1, Mat mat2)
        {
            double correl = Cv2.CompareHist(mat1, mat2, HistCompMethods.Correl);            
            return correl > 0.95;
        }

        private static void InsertValidImages()
        {
            foreach (var image in UnknowIsValidImages)
            {
                ValidImages.InsertValid(image.url);
            }
        }
    }
}
