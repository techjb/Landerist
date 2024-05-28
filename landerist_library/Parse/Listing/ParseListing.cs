using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using landerist_library.Parse.Listing.VertexAI;

namespace landerist_library.Parse.Listing
{
    public class ParseListing
    {
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseText(Page page)
        {
            var text = UserTextInput.GetText(page);
            if (string.IsNullOrEmpty(text))
            {
                return (PageType.ResponseBodyTooShort, null);
            }
            if (Configuration.Config.USE_VERTEX_AI_TO_PARSE_TEXT)
            {
                return ParseTextVertextAI(page, text);
            }
            return ParseTextChatGPT(page, text);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextVertextAI(Page page, string text)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            var response = VertexAIRequest.GetResponse(text).Result;
            if (response == null)
            {
                return result;
            }
            var (functionName, arguments) = VertexAIRequest.GetFunctionNameAndArguments(response);
            return ParseText(page, functionName, arguments);
        }


        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextChatGPT(Page page, string text)
        {
            var response = ChatGPTRequest.GetResponse(text);
            if (response == null)
            {
                return (PageType.MayBeListing, null);
            }

            var (functionName, arguments) = ChatGPTRequest.GetFunctionNameAndArguments(response);
            return ParseText(page, functionName, arguments);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseText(Page page, string? functionName, string? arguments)
        {
            if (functionName != null && arguments != null)
            {
                switch (functionName)
                {
                    case ParseListingTool.FunctionNameIsNotListing:
                        {
                            return (PageType.NotListingByParser, null);
                        }
                    case ParseListingTool.FunctionNameIsListing:
                        {
                            return ParseListingResponse.ParseListing(page, arguments);
                        }
                }
            }
            return (PageType.MayBeListing, null);
        }
    }
}
