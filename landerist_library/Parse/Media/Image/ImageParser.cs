using HtmlAgilityPack;
using landerist_library.Database;
using landerist_orels;
using OpenCvSharp;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Media.Image
{
    /// <summary>
    /// In Linux need to add another package. See:
    /// https://stackoverflow.com/questions/44105973/opencvsharp-unable-to-load-dll-opencvsharpextern
    /// </summary>
    public partial class ImageParser(MediaParser mediaParser)
    {
        public readonly MediaParser MediaParser = mediaParser;

        private const int MIN_IMAGE_SIZE = 256 * 256;

        protected readonly SortedSet<landerist_orels.Media> MediaImages = new(new MediaComparer());

        public readonly List<landerist_orels.Media> MediaToRemove = [];

        public readonly SortedSet<landerist_orels.Media> UnknowIsValidImages = new(new MediaComparer());

        public readonly Dictionary<Uri, Mat> DictionaryMats = [];

        public readonly Dictionary<Uri, Mat> NotDuplicatedMats = [];

        private static readonly HashSet<string> ProhibitedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "icon",
            "logo",
        };

        private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
        };

        public void AddImages()
        {
            AddImagesOpenGraph();
            AddImagesImgSrc();
            //GetImagesA(); // Add some invalid results
            RemoveImages();
            foreach (var image in MediaImages)
            {
                MediaParser.Add(image);
            }
        }

        protected void AddImagesOpenGraph()
        {
            var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image']");
            AddImages(imageNodes, "content");

            imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image:secure_url']");
            AddImages(imageNodes, "content");
        }

        private void AddImagesImgSrc()
        {
            var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//img");
            AddImages(imageNodes, "src");
        }

        //private void AddImagesA()
        //{
        //    var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//a");
        //    AddImages(imageNodes, "href");
        //}

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
            string attributeValue = imgNode.GetAttributeValue(name, "");
            if (string.IsNullOrWhiteSpace(attributeValue))
            {
                return;
            }

            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return;
            }

            if (!IsSupportedImageExtension(uri))
            {
                return;
            }

            if (!IsValidImageUri(uri))
            {
                return;
            }

            string title = MediaParser.GetTitle(imgNode);

            var media = new landerist_orels.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };

            MediaImages.Add(media);
        }

        public static bool IsValidImageUri(Uri uri)
        {
            string? filename = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (string.IsNullOrEmpty(filename))
            {
                return true;
            }

            filename = Uri.UnescapeDataString(filename);

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

        private static bool IsSupportedImageExtension(Uri uri)
        {
            string extension = Path.GetExtension(uri.AbsolutePath);
            return !string.IsNullOrEmpty(extension) && SupportedImageExtensions.Contains(extension);
        }

        private static bool FileNameContainsProhibitedWord(string fileName)
        {
            string filenameParsed = fileName.Replace("_", "-");
            var splitted = filenameParsed.Split('-', StringSplitOptions.RemoveEmptyEntries);

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
            Regex regex = RegexFileNameWidthXHeigth();
            Match match = regex.Match(fileName);

            if (!match.Success)
            {
                return false;
            }

            if (match.Groups.Count != 3)
            {
                return false;
            }

            string widthString = match.Groups[1].Value;
            string heightString = match.Groups[2].Value;

            if (!int.TryParse(widthString, out int width) ||
                !int.TryParse(heightString, out int height))
            {
                return false;
            }

            return ImageIsSmall(width, height);
        }

        private static bool ImageIsSmall(int width, int height)
        {
            long size = (long)width * height;
            return size < MIN_IMAGE_SIZE;
        }

        private void RemoveImages()
        {
            try
            {
                RemoveInvalidImages();
                LoadUnknowIsValid();
                new ImageDownloader(this).DownloadImages();
                RemoveSmallImages();
                new DuplicatesRemover(this).RemoveDuplicatedImages();
                InsertValidImages();
            }
            finally
            {
                DisposeLoadedMats();
            }
        }

        public void ProcessMediaToRemove(bool addToDiscarded)
        {
            foreach (var image in MediaToRemove)
            {
                MediaImages.Remove(image);
                UnknowIsValidImages.Remove(image);
                if (addToDiscarded)
                {
                    ValidInvalidImages.InsertInvalid(image.url);
                }
            }

            MediaToRemove.Clear();
        }

        private void RemoveInvalidImages()
        {
            foreach (var image in MediaImages)
            {
                if (ValidInvalidImages.IsInvalid(image.url))
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
                if (!ValidInvalidImages.IsValid(image.url))
                {
                    UnknowIsValidImages.Add(image);
                }
            }
        }

        private void RemoveSmallImages()
        {
            foreach (var image in UnknowIsValidImages)
            {
                if (!DictionaryMats.TryGetValue(image.url, out Mat? mat))
                {
                    continue;
                }

                if (ImageIsSmall(mat.Width, mat.Height))
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
                ValidInvalidImages.InsertValid(image.url);
            }
        }

        private void DisposeLoadedMats()
        {
            var mats = new HashSet<Mat>(ReferenceEqualityComparer.Instance);

            foreach (var mat in DictionaryMats.Values)
            {
                mats.Add(mat);
            }

            foreach (var mat in NotDuplicatedMats.Values)
            {
                mats.Add(mat);
            }

            foreach (var mat in mats)
            {
                mat.Dispose();
            }

            DictionaryMats.Clear();
            NotDuplicatedMats.Clear();
        }

        [GeneratedRegex(@"(\d+)x(\d+)")]
        private static partial Regex RegexFileNameWidthXHeigth();
    }
}
