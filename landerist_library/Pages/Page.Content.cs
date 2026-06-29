using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Downloaders;
using System.IO.Compression;
using System.Text;

namespace landerist_library.Pages
{
    public partial class Page
    {
        public HtmlDocument? GetHtmlDocument()
        {

            if (HtmlDocument != null && OriginalOuterHtml != null &&
                OriginalOuterHtml.Equals(HtmlDocument.DocumentNode.OuterHtml))
            {
                return HtmlDocument;
            }
            if (!string.IsNullOrEmpty(ResponseBody))
            {
                HtmlDocument = null;
                OriginalOuterHtml = null;

                try
                {
                    HtmlDocument = new();
                    HtmlDocument.LoadHtml(ResponseBody);
                    OriginalOuterHtml = HtmlDocument.DocumentNode.OuterHtml;
                    return HtmlDocument;
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteError("Page GetHtmlDocument", Uri, exception);
                }
            }
            return null;
        }

        public bool ResponseBodyIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(ResponseBody);
        }

        public void SetDownloadedData(IDownloader downloader)
        {
            var previousEtag = NormalizeHeaderValue(Etag);
            var downloadedEtag = NormalizeHeaderValue(downloader.Etag);
            var previousLastModified = NormalizeHeaderValue(LastModified);
            var downloadedLastModified = NormalizeHeaderValue(downloader.LastModified);

            HasComparableEtag = !string.IsNullOrEmpty(previousEtag) && !string.IsNullOrEmpty(downloadedEtag);
            EtagNotChanged = HasComparableEtag && string.Equals(previousEtag, downloadedEtag, StringComparison.Ordinal);
            HasComparableLastModified = !HasComparableEtag &&
                !string.IsNullOrEmpty(previousLastModified) &&
                !string.IsNullOrEmpty(downloadedLastModified);
            LastModifiedNotChanged = HasComparableLastModified &&
                string.Equals(previousLastModified, downloadedLastModified, StringComparison.Ordinal);

            ResponseBody = downloader.Content;
            ResetResponseBodyDerivedData();
            Screenshot = downloader.Screenshot;
            HttpStatusCode = downloader.HttpStatusCode;
            RedirectUrl = downloader.RedirectUrl;
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
                Screenshot.Length < Config.MAX_SCREENSHOT_SIZE;
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
            catch (Exception exception)
            {
                Logs.Log.WriteError("Page SetResponseBodyZipped", exception);
                return false;
            }
        }

        public void SetResponseBodyFromZipped()
        {
            if (ResponseBodyZipped is null)
            {
                return;
            }
            try
            {
                using var memoryStream = new MemoryStream(ResponseBodyZipped);
                using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                using var streamReader = new StreamReader(gzipStream);
                ResponseBody = streamReader.ReadToEnd();
                ResetResponseBodyDerivedData();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Page SetResponseBodyFromZipped", exception);
            }
        }

        public string? GetListingParserInput()
        {
            if (!string.IsNullOrEmpty(ListingParserInput))
            {
                return ListingParserInput;
            }

            ListingParserInput = Parse.ListingParser.UserInput.ParseListingUserInput.GetHtml(this);

            return ListingParserInput;
        }

        private void ResetResponseBodyDerivedData()
        {
            ListingParserInput = null;
            HtmlDocument = null;
            OriginalOuterHtml = null;
        }

        

        private static string? NormalizeHeaderValue(string? headerValue)
        {
            return string.IsNullOrWhiteSpace(headerValue) ? null : headerValue.Trim();
        }
    }
}
