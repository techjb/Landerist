using Google.Cloud.AIPlatform.V1;
using landerist_library.Parse.Listing.Anthropic;
using landerist_library.Parse.Listing.Gemini;
using landerist_library.Parse.Listing.OpenAI;
using landerist_library.Parse.Listing.VertexAI;
using landerist_library.Websites;
using landerist_library.Configuration;

namespace landerist_library.Parse.Listing
{
    public enum LLMProviders
    {
        OpenAI,
        Gemini,
        VertexAI,
        Anthropic
    }

    public class ParseListing
    {

        public static bool TooManyTokens(Page page)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProviders.OpenAI: return OpenAIRequest.TooManyTokens(page);
                case LLMProviders.VertexAI: return VertexAIRequest.TooManyTokens(page);                
                case LLMProviders.Gemini: return GeminiRequest.TooManyTokens(page);
                case LLMProviders.Anthropic: return AnthropicRequest.TooManyTokens(page);
                default:
                    break;
            }
            return false;
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            var text = UserTextInput.GetText(page);
            if (!string.IsNullOrEmpty(text))
            {
                switch (Config.LLM_PROVIDER)
                {
                    case LLMProviders.OpenAI: return ParseOpenAI(page, text);
                    case LLMProviders.VertexAI: return ParseVertextAI(page, text);
                    //case ModelName.Gemini: return GeminiRequest.ParseTextGemini(page);
                    case LLMProviders.Anthropic: return ParseAnthropic(page, text);                    
                    default:
                        break;

                }
            }
            return (PageType.ResponseBodyTooShort, null);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseOpenAI(Page page, string text)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.BulkParsing, null);
            }

            var response = OpenAIRequest.GetResponse(text);
            if (response == null)
            {
                return (PageType.MayBeListing, null);
            }

            var (functionName, arguments) = OpenAIResponse.GetFunctionNameAndArguments(response);
            return ParseText(page, functionName, arguments);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseVertextAI(Page page, string text)
        {
            var response = VertexAIRequest.GetResponse(page, text).Result;
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
            var (functionName, arguments) = VertexAIResponse.GetFunctionNameAndArguments(generateContentResponse);
            return ParseText(page, functionName, arguments);
        }

       
        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseAnthropic(Page page, string text)
        {
            var response = AnthropicRequest.GetResponse(page, text);
            if (response == null)
            {
                return (PageType.MayBeListing, null);
            }

            var (functionName, arguments) = AnthropicResponse.GetFunctionNameAndArguments(response);
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
