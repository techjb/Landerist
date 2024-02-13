using landerist_library.Websites;
using OpenAI;
using OpenAI.Chat;
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

        public static bool TooManyTokens(string? text)
        {
            if (text == null)
            {
                return false;
            }
            return TooManyTokens(SystemMessage, text);
        }

        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            var chatResponse = GetResponse(page.ResponseBodyText, false);
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (chatResponse == null)
            {
                return result;
            }

            var tool = GetTool(chatResponse);
            if (tool == null)
            {
                return result;
            }
            switch (tool.Function.Name)
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

        private static Tool? GetTool(ChatResponse chatResponse)
        {
            try
            {
                return chatResponse.FirstChoice.Message.ToolCalls[0];                
            }
            catch 
            {
            }
            return null;
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
