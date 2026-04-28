using HtmlAgilityPack;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser.UserInput
{
    public class ParseListingUserInput
    {
        public static string? GetHtml(string responseBody)
        {
            return GetText(responseBody, true);
        }

        public static string? GetText(string responseBody, bool html)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            if (html)
            {
                return GetHtml(htmlDocument, null);
            }

            return GetText(htmlDocument, null, null);
        }

        public static string? GetText(Page page)
        {
            try
            {
                var htmlDocument = page.GetHtmlDocument();
                if (htmlDocument != null)
                {
                    return GetText(htmlDocument, page.Website, page.Uri?.ToString());
                }
            }
            catch (Exception exception)
            {
                LogError("GetText(Page)", page.Uri?.ToString(), exception);
            }

            return null;
        }

        public static string? GetHtml(Page page)
        {
            try
            {
                var htmlDocument = page.GetHtmlDocument();
                if (htmlDocument != null)
                {
                    return GetHtml(htmlDocument, page.Website, page.Uri?.ToString());
                }
            }
            catch (Exception exception)
            {
                LogError("GetHtml(Page)", page.Uri?.ToString(), exception);
            }

            return null;
        }

        private static string? GetHtml(HtmlDocument htmlDocument, Website? website, string? context = null)
        {
            string? text = null;
            try
            {
                var workingDocument = Clone(htmlDocument);
                string? structuredData = ListingStructuredDataExtractor.Extract(workingDocument);
                ListingHtmlNodeRemover.RemoveBaseNoise(workingDocument);
                ListingHtmlNoiseRemover.Remove(workingDocument);
                ListingWebsiteHtmlNodeRemover.Remove(workingDocument, website, context);
                ListingHiddenContentRemover.Remove(workingDocument, preserveMediaNodes: true);
                ListingStructuredDataInjector.Prepend(workingDocument, structuredData);
                ListingHtmlAttributeCleaner.Clean(workingDocument);
                ListingEmptyElementRemover.Remove(workingDocument);
                text = ListingInputCleaner.CleanHtml(workingDocument);
                return text;
            }
            catch (Exception exception)
            {
                LogError("GetHtml(HtmlDocument)", context, exception);
            }

            return text;
        }

        public static string? GetText(HtmlDocument htmlDocument)
        {
            return GetText(htmlDocument, null, null);
        }

        private static string? GetText(HtmlDocument htmlDocument, Website? website, string? context)
        {
            try
            {
                var workingDocument = Clone(htmlDocument);
                string? structuredData = ListingStructuredDataExtractor.Extract(workingDocument);
                ListingHtmlNodeRemover.RemoveBaseNoise(workingDocument);
                ListingHtmlNoiseRemover.RemoveTags(workingDocument);
                ListingWebsiteHtmlNodeRemover.Remove(workingDocument, website, context);
                ListingHiddenContentRemover.Remove(workingDocument, preserveMediaNodes: false);

                string text = ListingVisibleTextExtractor.GetText(workingDocument);
                if (!string.IsNullOrWhiteSpace(structuredData))
                {
                    text = structuredData + Environment.NewLine + text;
                }

                return ListingInputCleaner.CleanText(text);
            }
            catch (Exception exception)
            {
                LogError("GetText(HtmlDocument)", context, exception);
            }

            return null;
        }

        private static HtmlDocument Clone(HtmlDocument htmlDocument)
        {
            var clone = new HtmlDocument();
            clone.LoadHtml(htmlDocument.DocumentNode.OuterHtml);
            return clone;
        }

        private static void LogError(string phase, string? context, Exception exception)
        {
            string source = "ParseListingUserInput " + phase;
            if (!string.IsNullOrWhiteSpace(context))
            {
                source += " " + context;
            }

            Logs.Log.WriteError(source, exception);
        }
    }
}
