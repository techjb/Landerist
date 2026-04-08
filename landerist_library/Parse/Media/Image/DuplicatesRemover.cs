using OpenCvSharp;

namespace landerist_library.Parse.Media.Image
{
    public class DuplicatesRemover(ImageParser imageParser)
    {
        private readonly ImageParser ImageParser = imageParser;

        public void RemoveDuplicatedImages()
        {
            FindDuplicates();

            foreach (var image in ImageParser.UnknowIsValidImages)
            {
                if (!ImageParser.NotDuplicatedMats.ContainsKey(image.url))
                {
                    ImageParser.MediaToRemove.Add(image);
                }
            }

            ImageParser.ProcessMediaToRemove(true);
        }

        private void FindDuplicates()
        {
            // Avoid modifying DictionaryMats while enumerating it.
            var histogramByUrl = new Dictionary<Uri, Mat>(ImageParser.DictionaryMats.Count);

            foreach (var (url, mat) in ImageParser.DictionaryMats)
            {
                histogramByUrl[url] = CalculateHistogram(mat);
            }

            ImageParser.DictionaryMats.Clear();

            foreach (var (url, hist) in histogramByUrl)
            {
                ImageParser.DictionaryMats[url] = hist;
            }

            foreach (var image in ImageParser.UnknowIsValidImages)
            {
                GetDuplicates(image);
            }
        }

        private void GetDuplicates(landerist_orels.Media image)
        {
            if (!ImageParser.DictionaryMats.TryGetValue(image.url, out Mat? currentMat) || currentMat is null)
            {
                return;
            }

            var existingImage = ImageParser.NotDuplicatedMats
                .FirstOrDefault(kvp => kvp.Value is not null && AreSimilar(kvp.Value, currentMat));

            if (existingImage.Key is null)
            {
                ImageParser.NotDuplicatedMats[image.url] = currentMat;
                return;
            }

            if (existingImage.Value.Width * existingImage.Value.Height < currentMat.Width * currentMat.Height)
            {
                ImageParser.NotDuplicatedMats.Remove(existingImage.Key);
                ImageParser.NotDuplicatedMats[image.url] = currentMat;
            }
        }

        private static Mat CalculateHistogram(Mat mat)
        {
            Mat hist = new();
            Cv2.CalcHist([mat], [0], null, hist, 1, [256], [new Rangef(0, 256)]);
            return hist;
        }

        private static bool AreSimilar(Mat mat1, Mat mat2)
        {
            double correl = Cv2.CompareHist(mat1, mat2, HistCompMethods.Correl);
            return correl > 0.95;
        }
    }
}
