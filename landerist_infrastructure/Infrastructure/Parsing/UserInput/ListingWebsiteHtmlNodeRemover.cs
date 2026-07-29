using landerist_library.Application.Logging;
using HtmlAgilityPack;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Parsing.UserInput
{
    internal static class ListingWebsiteHtmlNodeRemover
    {
        public static void Remove(HtmlDocument htmlDocument, Website? website, string? context, IApplicationLogger logger)
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
                logger.WriteError(source, text + Environment.NewLine + exception);
            }
        }
    }
}
