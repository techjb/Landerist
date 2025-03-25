﻿using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Parse.Listing.Anthropic;
using landerist_library.Parse.Listing.Gemini;
using landerist_library.Parse.Listing.OpenAI;
using landerist_library.Parse.Listing.VertexAI;
using landerist_library.Websites;
using Newtonsoft.Json;
using OpenAI.Chat;

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
                    case LLMProviders.OpenAI: return ParseOpenAI(page, userInput);
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
            PageType pageType = PageType.MayBeListing;
            landerist_orels.ES.Listing? listing = null;

            if (Config.BATCH_ENABLED)
            {
                return (pageType, listing, true);
            }

            var response = OpenAIRequest.GetChatResponse(userInput);
            (pageType, listing) = ParseOpenAI(page, response);
            return (pageType, listing, false);
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseOpenAI(Page page, ChatResponse? chatResponse)
        {
            if (chatResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            return ParseResponse(page, chatResponse.FirstChoice);
        }


        private static (PageType pageType, landerist_orels.ES.Listing? listing, bool waitingAIParsing)
            ParseVertextAI(Page page, string text)
        {
            var generateContentResponse = VertexAIRequest.GetResponse(page, text).Result;
            var (pageType, listing) = ParseVertextAI(page, generateContentResponse);
            return (pageType, listing, false);
        }

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseVertextAI(Page page, GenerateContentResponse? generateContentResponse)
        {
            if (generateContentResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            string? responseText = VertexAIResponse.GetResponseText(generateContentResponse);
            return ParseResponse(page, responseText);
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

        private static (PageType pageType, landerist_orels.ES.Listing? listing) ParseResponse(Page page, string? text)
        {
            if (text is null)
            {
                return (PageType.MayBeListing, null);
            }
            try
            {
                var structuredOutput = JsonConvert.DeserializeObject<StructuredOutput>(text);
                if (structuredOutput != null)
                {
                    return structuredOutput.ParseListing(page);
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
