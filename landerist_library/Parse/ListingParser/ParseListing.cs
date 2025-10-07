using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;

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
            NullValueHandling = NullValueHandling.Include,
        };

        public static (PageType pageType, Listing? listing, bool waitingAIRequest)
            Parse(Page page)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var text = page.GetParseListingUserInput();
            if (!string.IsNullOrEmpty(text))
            {
                switch (Config.LLM_PROVIDER)
                {
                    case LLMProvider.OpenAI: return ParseOpenAI(page, text);
                    case LLMProvider.VertexAI: return ParseVertextAI(page, text);
                    case LLMProvider.LocalAI: return ParseLocalAI(page, text);
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

        public static (PageType pageType, Listing? listing, bool waitingAIRequest)
            ParseLocalAI(Page page, string text)
        {
            var response = new LocalAIRequest().GetResponse(text).Result;
            if (response == null)
            {
                Console.WriteLine("ParseListing ParseLocalAI response is null");
                return (PageType.MayBeListing, null, true);
            }
            string? responseText = response.GetResponseText();
            if (string.IsNullOrEmpty(responseText))
            {
                Console.WriteLine("ParseListing ParseLocalAI responseText is null or empty. Finish Reason: " + response.GetFinishReason() + " TokenCount: " + page.TokenCount);
                return (PageType.MayBeListing, null, true);
            }
            var (pageType, listing) = ParseResponse(page, responseText);
            if (pageType.Equals(PageType.MayBeListing))
            {
                Console.WriteLine("ParseListing ParseLocalAI pageType is MayBeListing. Finish Reason: " + response.GetFinishReason() + " TokenCount " + page.TokenCount);
            }
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
                StructuredOutputEs? structuredOutputEs = null;
                if (Config.LLM_PROVIDER.Equals(LLMProvider.VertexAI))
                {
                    var structuredOutputVertexAIEs = JsonConvert.DeserializeObject<StructuredOutputVertexAIEs>(text, JsonSerializerSettings);
                    if (structuredOutputVertexAIEs != null)
                    {
                        structuredOutputEs = structuredOutputVertexAIEs?.Parse();
                    }
                }
                else
                {
                    structuredOutputEs = JsonConvert.DeserializeObject<StructuredOutputEs>(text, JsonSerializerSettings);
                }
                if (structuredOutputEs != null)
                {
                    return new StructuredOutputEsParser(structuredOutputEs).Parse(page);
                }
                else
                {
                    throw new Exception("StructuredOutputEs is null");
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListing ParseResponse", exception.Message);
            }
            return (PageType.MayBeListing, null);
        }
    }
}
