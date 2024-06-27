using landerist_library.Configuration;
using PuppeteerSharp;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using landerist_library.Logs;

namespace landerist_library.Downloaders.Puppeteer
{
    public class PuppeteerScreenshot
    {
        public static async Task<byte[]?> TakeScreenshot(IPage browserPage, Websites.Page page)
        {
            if (!Config.TAKE_SCREENSHOT)
            {
                return null;
            }

            ScreenshotOptions screenshotOptions = new()
            {
                Type = Config.SCREENSHOT_TYPE,
                FullPage = true,
                OmitBackground = true,
            };
            if (Config.SCREENSHOT_TYPE.Equals(ScreenshotType.Jpeg)
                //|| Config.SCREENSHOT_TYPE.Equals(ScreenshotType.Webp) Not supported for webp. Screenshots have low quality.
                )
            {
                screenshotOptions.Quality = 90;
            }
            try
            {
                var data = await browserPage.ScreenshotDataAsync(screenshotOptions);
                if (data != null)
                {
                    data = ResizeImage(data);

                    if (Config.SAVE_SCREENSHOT_FILE)
                    {
                        string fileName = Config.SCREENSHOTS_DIRECTORY + page.UriHash + "." + Config.SCREENSHOT_TYPE.ToString().ToLower();
                        File.WriteAllBytes(fileName, data);
                    }
                    return data;
                }
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("PuppeteerDownloader TakeScreenshot", exception);
            }
            return null;
        }

#pragma warning disable CA1416 // only supported in windows
        static byte[] ResizeImage(byte[] bytes)
        {
            try
            {
                using MemoryStream memoryStream = new(bytes);
                using Image image = Image.FromStream(memoryStream);

                bytes = ResizeImageToMaxSides(bytes, image);
                bytes = ResizeImageToMaxSize(bytes, image);
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("PuppeteerScreenshot ResizeImage", exception);
            }
            return bytes;
        }


        static byte[] ResizeImageToMaxSides(byte[] bytes, Image image)
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            if (originalWidth <= Config.MAX_SCREENSHOT_PIXELS_SIDE &&
                originalHeight <= Config.MAX_SCREENSHOT_PIXELS_SIDE)
            {
                return bytes;
            }

            float heightWidthRatio = originalWidth / (float)originalHeight;
            int newWidth, newHeight;

            if (originalWidth > originalHeight)
            {
                newWidth = Config.MAX_SCREENSHOT_PIXELS_SIDE;
                newHeight = (int)(Config.MAX_SCREENSHOT_PIXELS_SIDE / heightWidthRatio);
            }
            else
            {
                newHeight = Config.MAX_SCREENSHOT_PIXELS_SIDE;
                newWidth = (int)(Config.MAX_SCREENSHOT_PIXELS_SIDE * heightWidthRatio);
            }

            using Bitmap resizedImage = new(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            using MemoryStream resizedMemoryStream = new();
            ImageFormat imageFormat = Config.SCREENSHOT_TYPE.Equals(ScreenshotType.Jpeg) ?
                ImageFormat.Jpeg :
                ImageFormat.Png;

            resizedImage.Save(resizedMemoryStream, imageFormat);
            return resizedMemoryStream.ToArray();
        }


        static byte[] ResizeImageToMaxSize(byte[] bytes, Image image)
        {
            if (bytes.Length < Config.MAX_SCREENSHOT_SIZE)
            {
                return bytes;
            }

            switch (Config.SCREENSHOT_TYPE)
            {
                case ScreenshotType.Jpeg: return ResizeImageToMaxSizeJpeg(image);
                case ScreenshotType.Png: return ResizeImageToMaxSizePng(bytes, image);
                case ScreenshotType.Webp:
                    break;
            }
            return [];
        }

        static byte[] ResizeImageToMaxSizeJpeg(Image image)
        {
            int quality = 100;
            byte[] resizedBytes;

            do
            {
                using MemoryStream outputStream = new();
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                image.Save(outputStream, jpgEncoder, encoderParams);
                resizedBytes = outputStream.ToArray();

                quality -= 5;
            } while (resizedBytes.Length > Config.MAX_SCREENSHOT_SIZE && quality > 0);

            return resizedBytes;
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        static byte[] ResizeImageToMaxSizePng(byte[] bytes, Image image)
        {
            int width = image.Width;
            int height = image.Height;

            double scale = Math.Sqrt((double)Config.MAX_SCREENSHOT_SIZE / bytes.Length);
            int newWidth = (int)(width * scale);
            int newHeight = (int)(height * scale);

            using Bitmap resizedBitmap = new(image, new Size(newWidth, newHeight));
            using MemoryStream outputStream = new();
            resizedBitmap.Save(outputStream, ImageFormat.Png);
            byte[] resizedBytes = outputStream.ToArray();

            while (resizedBytes.Length > Config.MAX_SCREENSHOT_SIZE)
            {
                scale *= 0.9;
                newWidth = (int)(width * scale);
                newHeight = (int)(height * scale);

                using Bitmap furtherResizedBitmap = new(image, new Size(newWidth, newHeight));
                outputStream.SetLength(0);
                furtherResizedBitmap.Save(outputStream, ImageFormat.Png);
                resizedBytes = outputStream.ToArray();
            }

            return resizedBytes;
        }

#pragma warning restore CA1416

    }
}
