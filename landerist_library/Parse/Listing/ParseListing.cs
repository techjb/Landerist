using Google.Cloud.AIPlatform.V1;
using landerist_library.Parse.Listing.Anthropic;
using landerist_library.Parse.Listing.Gemini;
using landerist_library.Parse.Listing.OpenAI;
using landerist_library.Parse.Listing.VertexAI;
using landerist_library.Websites;
using landerist_library.Configuration;
using OpenAI.Chat;
using OpenCvSharp;

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

        public static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing) 
            Parse(Page page)
        {
            var userInput = UserTextInput.GetText(page);
            if (!string.IsNullOrEmpty(userInput))
            {
                switch (Config.LLM_PROVIDER)
                {
                    case LLMProviders.OpenAI: return ParseOpenAIStructured(page, userInput);
                    //case LLMProviders.OpenAI: return ParseOpenAI(page, userInput);
                    case LLMProviders.VertexAI: return ParseVertextAI(page, userInput);
                    //case ModelName.Gemini: return GeminiRequest.ParseTextGemini(page);
                    case LLMProviders.Anthropic: return ParseAnthropic(page, userInput);                    
                    default:
                        break;

                }
            }
            return (PageType.ResponseBodyTooShort, null, false);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing)
            ParseOpenAI(Page page, string userInput)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var response = OpenAIRequest.GetResponse(userInput);
            return ParseOpenAI(page, response);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing)
            ParseOpenAIStructured(Page page, string userInput)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var structuredOutput = OpenAIRequest.GetStructuredOutput(userInput);
            return ParseOpenAI(page, structuredOutput);
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing) ParseOpenAI(Page page, ChatResponse? chatResponse)
        {
            if (chatResponse == null)
            {
                return (PageType.MayBeListing, null, false);
            }

            var (functionName, arguments) = OpenAIResponse.GetFunctionNameAndArguments(chatResponse);
            var (pageType, listing) = ParseText(page, functionName, arguments);
            return (pageType, listing, false);
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing) ParseOpenAI(Page page, OpenAIStructuredOutput? structuredOutput)
        {
            if (structuredOutput == null)
            {
                return (PageType.MayBeListing, null, false);
            }
            
            if (!structuredOutput.EsUnAnuncio)
            {
                return (PageType.NotListingByParser, null, false);
            }
            return (PageType.Listing, null, false);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing) 
            ParseVertextAI(Page page, string text)
        {
            var response = VertexAIRequest.GetResponse(page, text).Result;
            var (pageType, listing) = ParseTextVertextAI(page, response);
            return (pageType, listing, false);
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseTextVertextAIFromBatch(Page page, string userInput)
        {
            var response = VertexAIBatch.GetGenerateContentResponse(userInput);
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

       
        private static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing) 
            ParseAnthropic(Page page, string text)
        {
            var response = AnthropicRequest.GetResponse(page, text);
            if (response == null)
            {
                return (PageType.MayBeListing, null, false);
            }

            var (functionName, arguments) = AnthropicResponse.GetFunctionNameAndArguments(response);
            var (pageType, listing) = ParseText(page, functionName, arguments);
            return (pageType, listing, false);
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
