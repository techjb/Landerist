using landerist_library.Websites;
using OpenAI;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingRequest : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Analiza detenidamente el texto proporcionado por el usuario. " +
            "Determina si se trata o no de un único anuncio inmobiliario. " +
            "En caso de que sea un anuncio inmobiliario, extrae de manera precisa los elementos clave en formato json. " +
            "Mantente enfocado y da tu mejor respuesta.";

        private static readonly List<Tool> Tools = ParseListingTool.GetTools();

        public ParseListingRequest() : base(SystemMessage, Tools)
        {

        }

        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            var chatResponse = GetResponse(page.ResponseBodyText, false);
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (chatResponse == null)
            {
                return result;
            }
            if (chatResponse.FirstChoice.Message.ToolCalls.Count <= 0)
            {
                return result;
            }

            var tool = chatResponse.FirstChoice.Message.ToolCalls[0];
            string functionName = tool.Function.Name;
            switch (functionName)
            {
                case ParseListingTool.FunctionNameIsNotListing:
                    {
                        result.pageType = PageType.NotListingByParser;
                    }
                    break;
                case ParseListingTool.FunctionNameIsListing:
                    {
                        result = ParseListing(page, tool);
                    }
                    break;
            }

            return result;
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseListing(Page page, Tool tool)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            try
            {
                string arguments = tool.Function.Arguments.ToString();
                var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(arguments);
                if (parseListingResponse != null)
                {
                    result.pageType = PageType.ListingButNotParsed;
                    result.listing = parseListingResponse.ToListing(page);
                    if (result.listing != null)
                    {
                        result.pageType = PageType.Listing;
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ParseListingRequest ParseListing", page.Uri, exception);
            }

            return result;
        }
    }
}
