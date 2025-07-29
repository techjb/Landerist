using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;
using System.Reflection;
using landerist_library.Parse.ListingParser.LocalAI;

namespace landerist_library.Parse.ListingParser
{
    public enum LLMProvider
    {
        OpenAI,
        //Gemini,
        VertexAI,
        //Anthropic,
        LocalAI,
    }

    public class ParseListing
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        public static (PageType pageType, Listing? listing, bool waitingAIRequest)
            Parse(Page page)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var userInput = ParseListingUserInput.GetText(page);
            if (!string.IsNullOrEmpty(userInput))
            {
                switch (Config.LLM_PROVIDER)
                {
                    case LLMProvider.OpenAI: return ParseOpenAI(page, userInput);
                    case LLMProvider.VertexAI: return ParseVertextAI(page, userInput);
                    case LLMProvider.LocalAI: return ParseLocalAI(page, userInput);
                    default:
                        break;

                }
            }
            return (PageType.ResponseBodyTooShort, null, false);
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest)
            ParseOpenAI(Page page, string userInput)
        {
            var response = OpenAIRequest.GetChatResponse(userInput);
            if (response == null || response.FirstChoice == null)
            {
                return (PageType.MayBeListing, null, true);
            }
            var (pageType, listing) = ParseResponse(page, response.FirstChoice);
            return (pageType, listing, false);
        }


        private static (PageType pageType, Listing? listing, bool waitingAIRequest)
            ParseVertextAI(Page page, string text)
        {
            var generateContentResponse = VertexAIRequest.GetResponse(page, text).Result;
            if (generateContentResponse == null)
            {
                return (PageType.MayBeListing, null, true);
            }
            string? responseText = VertexAIResponse.GetResponseText(generateContentResponse);
            var (pageType, listing) = ParseResponse(page, responseText);
            return (pageType, listing, false);
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest)
            ParseLocalAI(Page page, string text)
        {
            var response = new LocalAIRequest().GetResponse(text).Result;
            if (response == null)
            {
                return (PageType.MayBeListing, null, true);
            }
            string? responseText = response.GetResponseText();
            var (pageType, listing) = ParseResponse(page, responseText);
            return (pageType, listing, false);
        }

        public static (PageType pageType, Listing? listing) ParseResponse(Page page, string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (PageType.MayBeListing, null);
            }
            try
            {
                var structuredOutput = JsonConvert.DeserializeObject<StructuredOutputEs>(text, JsonSerializerSettings);
                if (structuredOutput != null)
                {
                    return new StructuredOutputEsParser(structuredOutput).Parse(page);
                }
                Logs.Log.WriteError("ParseListing ParseResponse", "StructuredOutput null");
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListing ParseResponse", exception.Message);
            }
            return (PageType.MayBeListing, null);
        }
    }
}
