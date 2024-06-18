using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Parse.Listing.VertexAI;
using landerist_library.Websites;
using Google.Cloud.AIPlatform.V1;

namespace landerist_library.Parse.Listing
{
    public class ParseListing
    {
        public static bool TooManyTokens(Page page)
        {
            return VertexAIRequest.TooManyTokens(page);
            //return ChatGPTRequest.TooManyTokens(page);
        }
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseText(Page page)
        {
            var text = UserTextInput.GetText(page);
            if (string.IsNullOrEmpty(text))
            {
                return (PageType.ResponseBodyTooShort, null);
            }
            return ParseTextVertextAI(page, text);
            //return ParseTextChatGPT(page, text);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextVertextAI(Page page, string text)
        {
            var response = VertexAIRequest.GetResponse(text).Result;
            Console.WriteLine("Parsing vertex ai..");
            return ParseTextVertextAI(page, response);
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextVertextAIFromBatch(Page page, string text)
        {
            var response = VertexAIBatch.GetGenerateContentResponse(text);
            return ParseTextVertextAI(page, response);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextVertextAI(Page page, GenerateContentResponse? generateContentResponse)
        {
            if (generateContentResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            var (functionName, arguments) = VertexAIRequest.GetFunctionNameAndArguments(generateContentResponse);
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
