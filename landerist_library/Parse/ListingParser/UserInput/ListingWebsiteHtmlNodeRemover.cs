using HtmlAgilityPack;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingWebsiteHtmlNodeRemover
    {
        public static void Remove(HtmlDocument htmlDocument, Website? website, string? context)
        {
            string? removeXPath = website?.ListingHtmlRemoveXPath;
            if (string.IsNullOrWhiteSpace(removeXPath))
            {
                return;
            }

            try
            {
                ListingHtmlNodeRemover.Remove(htmlDocument, removeXPath);
            }
            catch (Exception exception)
            {
                string source = "ListingWebsiteHtmlNodeRemover Remove";
                string text = website?.Host ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(context))
                {
                    text += " " + context;
                }

                text += " " + removeXPath;
                Logs.Log.WriteError(source, text, exception);
            }
        }
    }
}
