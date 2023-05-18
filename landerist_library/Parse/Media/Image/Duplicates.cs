using OpenCvSharp;

namespace landerist_library.Parse.Media.Image
{
    public class Duplicates
    {
        private readonly ImageParser ImageParser;
        public Duplicates(ImageParser imageParser)
        {
            ImageParser = imageParser;

        }
        public void FindDuplicates()
        {
            foreach (var kvp in ImageParser.DictionaryMats)
            {
                ImageParser.DictionaryMats[kvp.Key] = CalculateHystogram(kvp.Value);
            }
            foreach (var image in ImageParser.UnknowIsValidImages)
            {
                GetDuplicates(image);
            }
        }

        private void GetDuplicates(landerist_orels.ES.Media image)
        {
            if (!ImageParser.DictionaryMats.TryGetValue(image.url, out Mat? currentMat))
            {
                return;
            }

            var existingImage = ImageParser.NotDuplicatedMats.FirstOrDefault(kvp => AreSimilar(kvp.Value, currentMat));
            if (existingImage.Equals(default(KeyValuePair<Uri, Mat>)))
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
    }
}
