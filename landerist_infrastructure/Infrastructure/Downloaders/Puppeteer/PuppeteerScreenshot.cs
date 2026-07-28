using landerist_library.Application.Logging;
using PuppeteerSharp;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer
{
    public class PuppeteerScreenshot
    {
        private readonly PuppeteerScreenshotPolicy Policy;
        private readonly IScreenshotStore Store;
        private readonly IApplicationLogger Logger;

        public PuppeteerScreenshot(
            PuppeteerScreenshotPolicy policy,
            IScreenshotStore store,
            IApplicationLogger logger)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(logger);
            Logger = logger;
            Policy = policy.Validate();
            Store = store;
        }
        public async Task<byte[]?> TakeScreenshot(IPage browserPage, Pages.Page page)
        {
            ScreenshotOptions screenshotOptions = new()
            {
                Type = Policy.Type,
                FullPage = true,
                OmitBackground = true,
            };
            if (Policy.Type.Equals(ScreenshotType.Jpeg)
                //|| Policy.Type.Equals(ScreenshotType.Webp) Not supported for webp. Screenshots have low quality.
                )
            {
                screenshotOptions.Quality = Policy.InitialJpegQuality;
            }
            try
            {
                var data = await browserPage.ScreenshotDataAsync(screenshotOptions);
                if (data != null)
                {
                    data = ResizeImage(data, page.MaxScreenshotSize);

                    Store.Save(page.UriHash, Policy.Type, data);
                    return data;
                }
            }
            catch (Exception exception)
            {
                Logger.WriteError("PuppeteerDownloader TakeScreenshot", exception.ToString());
            }
            return null;
        }

#pragma warning disable CA1416 // only supported in windows
        byte[] ResizeImage(byte[] bytes, int maxSizeBytes)
        {
            try
            {
                using MemoryStream memoryStream = new(bytes);
                using Image image = Image.FromStream(memoryStream);

                bytes = ResizeImageToMaxSides(bytes, image);
                bytes = ResizeImageToMaxSize(bytes, image, maxSizeBytes);
            }
            catch (Exception exception)
            {
                Logger.WriteError("PuppeteerScreenshot ResizeImage", exception.ToString());
            }
            return bytes;
        }


        byte[] ResizeImageToMaxSides(byte[] bytes, Image image)
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            if (originalWidth <= Policy.MaxPixelsPerSide &&
                originalHeight <= Policy.MaxPixelsPerSide)
            {
                return bytes;
            }

            float heightWidthRatio = originalWidth / (float)originalHeight;
            int newWidth, newHeight;

            if (originalWidth > originalHeight)
            {
                newWidth = Policy.MaxPixelsPerSide;
                newHeight = (int)(Policy.MaxPixelsPerSide / heightWidthRatio);
            }
            else
            {
                newHeight = Policy.MaxPixelsPerSide;
                newWidth = (int)(Policy.MaxPixelsPerSide * heightWidthRatio);
            }

            using Bitmap resizedImage = new(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            using MemoryStream resizedMemoryStream = new();
            ImageFormat imageFormat = Policy.Type.Equals(ScreenshotType.Jpeg) ?
                ImageFormat.Jpeg :
                ImageFormat.Png;

            resizedImage.Save(resizedMemoryStream, imageFormat);
            return resizedMemoryStream.ToArray();
        }


        byte[] ResizeImageToMaxSize(byte[] bytes, Image image, int maxSizeBytes)
        {
            if (bytes.Length < maxSizeBytes)
            {
                return bytes;
            }

            switch (Policy.Type)
            {
                case ScreenshotType.Jpeg: return ResizeImageToMaxSizeJpeg(image, maxSizeBytes);
                case ScreenshotType.Png: return ResizeImageToMaxSizePng(bytes, image, maxSizeBytes);
                case ScreenshotType.Webp:
                    break;
            }
            return [];
        }

        byte[] ResizeImageToMaxSizeJpeg(Image image, int maxSizeBytes)
        {
            int quality = 100;
            byte[] resizedBytes;

            do
            {
                using MemoryStream outputStream = new();
                ImageCodecInfo? jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                if (jpgEncoder == null)
                {
                    return [];
                }
                EncoderParameters encoderParams = new(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                image.Save(outputStream, jpgEncoder, encoderParams);
                resizedBytes = outputStream.ToArray();

                quality -= 5;
            } while (resizedBytes.Length > maxSizeBytes && quality > 0);

            return resizedBytes;
        }

        static ImageCodecInfo? GetEncoder(ImageFormat format)
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

        byte[] ResizeImageToMaxSizePng(byte[] bytes, Image image, int maxSizeBytes)
        {
            int width = image.Width;
            int height = image.Height;

            double scale = Math.Sqrt((double)maxSizeBytes / bytes.Length);
            int newWidth = (int)(width * scale);
            int newHeight = (int)(height * scale);

            using Bitmap resizedBitmap = new(image, new Size(newWidth, newHeight));
            using MemoryStream outputStream = new();
            resizedBitmap.Save(outputStream, ImageFormat.Png);
            byte[] resizedBytes = outputStream.ToArray();

            while (resizedBytes.Length > maxSizeBytes)
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
