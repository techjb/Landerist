using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingStructuredDataInjector
    {
        public static void Prepend(HtmlDocument htmlDocument, string? structuredData)
        {
            if (string.IsNullOrWhiteSpace(structuredData))
            {
                return;
            }

            var section = htmlDocument.CreateElement("section");
            section.SetAttributeValue("data-landerist-structured-data", "true");

            var heading = htmlDocument.CreateElement("h2");
            heading.AppendChild(htmlDocument.CreateTextNode("Structured page data"));
            section.AppendChild(heading);

            var pre = htmlDocument.CreateElement("pre");
            pre.AppendChild(htmlDocument.CreateTextNode(structuredData));
            section.AppendChild(pre);

            var body = htmlDocument.DocumentNode.SelectSingleNode("//body");
            if (body != null)
            {
                body.PrependChild(section);
                return;
            }

            htmlDocument.DocumentNode.PrependChild(section);
        }
    }
}
