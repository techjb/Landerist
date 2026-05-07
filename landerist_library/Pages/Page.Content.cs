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
            var previousEtag = NormalizeEtag(Etag);
            var downloadedEtag = NormalizeEtag(downloader.Etag);

            HasComparableEtag = !string.IsNullOrEmpty(previousEtag) && !string.IsNullOrEmpty(downloadedEtag);
            EtagNotChanged = HasComparableEtag && string.Equals(previousEtag, downloadedEtag, StringComparison.Ordinal);

            ResponseBody = downloader.Content;
            ResetResponseBodyDerivedData();
            Screenshot = downloader.Screenshot;
            HttpStatusCode = downloader.HttpStatusCode;
            RedirectUrl = downloader.RedirectUrl;
            Etag = downloadedEtag;
        }

        public bool EtagHasNotChanged()
        {
            if (HasComparableEtag)
            {
                return EtagNotChanged;
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

            ListingParserInput = Website.ApplySpecialRules || Config.IsConfigurationLocal() ?
                Parse.ListingParser.UserInput.ParseListingUserInput.GetHtml(this) :
                Parse.ListingParser.UserInput.ParseListingUserInput.GetText(this);

            //ListingParserInput = Parse.ListingParser.UserInput.ParseListingUserInput.GetHtml(this);
            //var text = this.GetHtmlDocument().Text;
            return ListingParserInput;
        }

        private void ResetResponseBodyDerivedData()
        {
            ListingParserInput = null;
            HtmlDocument = null;
            OriginalOuterHtml = null;
        }

        private static string? NormalizeEtag(string? etag)
        {
            return string.IsNullOrWhiteSpace(etag) ? null : etag.Trim();
        }
    }
}
