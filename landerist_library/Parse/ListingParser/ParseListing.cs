using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.Anthropic;
using landerist_library.Parse.ListingParser.Gemini;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace landerist_library.Parse.ListingParser
{
    public enum LLMProvider
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
                case LLMProvider.OpenAI: return OpenAIRequest.TooManyTokens(page);
                case LLMProvider.VertexAI: return VertexAIRequest.TooManyTokens(page);
                case LLMProvider.Gemini: return GeminiRequest.TooManyTokens(page);
                //case LLMProviders.Anthropic: return AnthropicRequest.TooManyTokens(page);
                default:
                    break;
            }
            return false;
        }

        public static (PageType pageType, Listing? listing, bool waitingAIParsing)
            Parse(Page page)
        {
            var userInput = ParseListingUserInput.GetText(page);
            if (!string.IsNullOrEmpty(userInput))
            {
                switch (Config.LLM_PROVIDER)
                {
                    case LLMProvider.OpenAI: return ParseOpenAI(page, userInput);
                    case LLMProvider.VertexAI: return ParseVertextAI(page, userInput);
                    //case ModelName.Gemini: return GeminiRequest.ParseTextGemini(page);
                    //case LLMProviders.Anthropic: return ParseAnthropic(page, userInput);
                    default:
                        break;

                }
            }
            return (PageType.ResponseBodyTooShort, null, false);
        }

        private static (PageType pageType, Listing? listing, bool waitingAIParsing)
            ParseOpenAI(Page page, string userInput)
        {
            PageType pageType = PageType.MayBeListing;
            Listing? listing = null;

            if (Config.BATCH_ENABLED)
            {
                return (pageType, listing, true);
            }

            var response = OpenAIRequest.GetChatResponse(userInput);
            (pageType, listing) = ParseOpenAI(page, response);
            return (pageType, listing, false);
        }

        public static (PageType pageType, Listing? listing) ParseOpenAI(Page page, ChatResponse? chatResponse)
        {
            if (chatResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            return ParseResponse(page, chatResponse.FirstChoice);
        }


        private static (PageType pageType, Listing? listing, bool waitingAIParsing)
            ParseVertextAI(Page page, string text)
        {
            var generateContentResponse = VertexAIRequest.GetResponse(page, text).Result;
            var (pageType, listing) = ParseVertextAI(page, generateContentResponse);
            return (pageType, listing, false);
        }

        private static (PageType pageType, Listing? listing) ParseVertextAI(Page page, GenerateContentResponse? generateContentResponse)
        {
            if (generateContentResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            string? responseText = VertexAIResponse.GetResponseText(generateContentResponse);
            return ParseResponse(page, responseText);
        }


        //private static (PageType pageType, Listing? listing, bool waitingAIParsing)
        //    ParseAnthropic(Page page, string text)
        //{
        //    var response = AnthropicRequest.GetResponse(page, text);
        //    if (response == null)
        //    {
        //        return (PageType.MayBeListing, null, false);
        //    }

        //    var (functionName, arguments) = AnthropicResponse.GetFunctionNameAndArguments(response);
        //    var (pageType, listing) = ParseAnthropic(page, functionName, arguments);
        //    return (pageType, listing, false);
        //}

        //private static (PageType pageType, Listing? listing) ParseAnthropic(Page page, string? functionName, string? arguments)
        //{
        //    if (functionName != null && arguments != null)
        //    {
        //        switch (functionName)
        //        {
        //            case StructuredOutputEsParse.FunctionNameIsNotListing:
        //                {
        //                    return (PageType.NotListingByParser, null);
        //                }
        //            case StructuredOutputEsParse.FunctionNameIsListing:
        //                {
        //                    return ParseListingResponse.ParseListing(page, arguments);
        //                }
        //        }
        //    }
        //    return (PageType.MayBeListing, null);
        //}

        private static (PageType pageType, Listing? listing) ParseResponse(Page page, string? text)
        {
            if (text is null)
            {
                return (PageType.MayBeListing, null);
            }
            try
            {
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };

                var structuredOutput = JsonConvert.DeserializeObject<StructuredOutputEs>(text, settings);                
                if (structuredOutput != null)
                {
                    return new StructuredOutputEsParser(structuredOutput).Parse(page);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListing ParseResponse", page.Uri, exception);
            }
            return (PageType.MayBeListing, null);
        }
    }
}
