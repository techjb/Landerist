using System.IO.Compression;
using System.Text;

namespace landerist_library.Pages
{
    public partial class Page
    {
        public string? GetResponseBody()
        {
            return ResponseBody;
        }
        public bool ResponseBodyIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(ResponseBody);
        }

        public void SetDownloadedData(PageDownloadResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            var previousEtag = NormalizeHeaderValue(Etag);
            var downloadedEtag = NormalizeHeaderValue(result.Etag);
            var previousLastModified = NormalizeHeaderValue(LastModified);
            var downloadedLastModified = NormalizeHeaderValue(result.LastModified);

            HasComparableEtag = !string.IsNullOrEmpty(previousEtag) && !string.IsNullOrEmpty(downloadedEtag);
            EtagNotChanged = HasComparableEtag && string.Equals(previousEtag, downloadedEtag, StringComparison.Ordinal);
            HasComparableLastModified = !HasComparableEtag &&
                !string.IsNullOrEmpty(previousLastModified) &&
                !string.IsNullOrEmpty(downloadedLastModified);
            LastModifiedNotChanged = HasComparableLastModified &&
                string.Equals(previousLastModified, downloadedLastModified, StringComparison.Ordinal);

            ResponseBody = result.Content;
            ResetResponseBodyDerivedData();
            Screenshot = result.Screenshot;
            HttpStatusCode = result.HttpStatusCode;
            RedirectUrl = result.RedirectUrl;
            Etag = downloadedEtag;
            LastModified = downloadedLastModified;
        }

        public bool EtagHasNotChanged()
        {
            return DownloadedHeadersHaveNotChanged();
        }

        public bool DownloadedHeadersHaveNotChanged()
        {
            if (HasComparableEtag)
            {
                return EtagNotChanged;
            }
            if (HasComparableLastModified)
            {
                return LastModifiedNotChanged;
            }
            return false;
        }

        public bool ContainsScreenshot()
        {
            return Screenshot != null &&
                Screenshot.Length > 0 &&
                Screenshot.Length < Rules.MaxScreenshotSize;
        }

        public void RemoveResponseBodyZipped()
        {
            ResponseBodyZipped = null;
        }

        public void RemoveResponseBody()
        {
            ResponseBody = null;
            ResetResponseBodyDerivedData();
        }

        public bool SetResponseBodyZipped()
        {
            if (string.IsNullOrEmpty(ResponseBody))
            {
                ResponseBodyZipped = null;
                return false;
            }

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(ResponseBody);
                using var memoryStream = new MemoryStream();
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }

                ResponseBodyZipped = memoryStream.ToArray();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SetResponseBodyFromZipped()
        {
            if (ResponseBodyZipped is null)
            {
                return false;
            }
            try
            {
                using var memoryStream = new MemoryStream(ResponseBodyZipped);
                using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                using var streamReader = new StreamReader(gzipStream);
                ResponseBody = streamReader.ReadToEnd();
                ResetResponseBodyDerivedData();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ResetResponseBodyDerivedData()
        {
            ListingParserInput = null;
        }

        

        private static string? NormalizeHeaderValue(string? headerValue)
        {
            return string.IsNullOrWhiteSpace(headerValue) ? null : headerValue.Trim();
        }
    }
}
