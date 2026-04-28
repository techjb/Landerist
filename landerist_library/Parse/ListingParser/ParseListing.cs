using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Statistics;
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

        public static (PageType pageType, Listing? listing, bool waitingAIRequest) Parse(Page page)
        {
            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var text = page.GetListingParserInput();
            if (string.IsNullOrWhiteSpace(text))
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }

            return Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => ParseOpenAI(page, text),
                LLMProvider.VertexAI => ParseVertexAI(page, text),
                LLMProvider.LocalAI => ParseLocalAI(page, text),
                _ => (PageType.ResponseBodyTooShort, null, false),
            };
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseOpenAI(Page page, string userInput)
        {
            return ParseWithRetry(page, () =>
            {
                var response = OpenAIRequest.GetChatResponse(userInput);
                if (response?.FirstChoice == null)
                {
                    return (PageType.MayBeListing, null, true);
                }

                var (pageType, listing) = ParseResponse(page, response.FirstChoice);
                return (pageType, listing, false);
            });
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseVertexAI(Page page, string text)
        {
            return ParseWithRetry(page, () =>
            {
                var generateContentResponse = VertexAIRequest.GetResponse(page, text).GetAwaiter().GetResult();
                if (generateContentResponse == null)
                {
                    return (PageType.MayBeListing, null, true);
                }

                var responseText = VertexAIResponse.GetResponseText(generateContentResponse);
                var (pageType, listing) = ParseResponse(page, responseText);
                return (pageType, listing, false);
            });
        }

        public static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseLocalAI(Page page, string text)
        {
            return ParseWithRetry(page, () =>
            {
                var response = new LocalAIRequest().GetResponse(text).GetAwaiter().GetResult();
                if (response == null)
                {
                    Console.WriteLine("ParseListing ParseLocalAI response is null.");
                    return (PageType.MayBeListing, null, true);
                }

                var responseText = response.GetResponseText();
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    Console.WriteLine("ParseListing ParseLocalAI responseText is null or empty. Finish Reason: " + response.GetFinishReason() + " TokenCount: " + page.TokenCount);
                    return (PageType.MayBeListing, null, true);
                }

                var (pageType, listing) = ParseResponse(page, responseText);
                if (pageType == PageType.MayBeListing)
                {
                    Console.WriteLine("ParseListing ParseLocalAI pageType is MayBeListing. Finish Reason: " + response.GetFinishReason() + " TokenCount " + page.TokenCount + " Uri: " + page.Uri);
                }

                return (pageType, listing, false);
            });
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseWithRetry(
            Page page,
            Func<(PageType pageType, Listing? listing, bool waitingAIRequest)> parse)
        {
            var result = parse();
            if (!ShouldRetryNotListing(page, result.pageType))
            {
                return result;
            }

            HostStatistics.InsertDailyCounter(page.Website.Host, HostStatisticsKey.ParseListingRetryNotListing);
            return parse();
        }

        private static bool ShouldRetryNotListing(Page page, PageType pageType)
        {
            return pageType == PageType.NotListingByParser
                && page.Website.MatchesListingUrlRegex(page.Uri);
        }

        public static (PageType pageType, Listing? listing) ParseResponse(Page page, string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (PageType.MayBeListing, null);
            }

            try
            {
                StructuredOutputEs? structuredOutputEs;
                if (Config.LLM_PROVIDER == LLMProvider.VertexAI)
                {
                    var structuredOutputVertexAIEs = JsonConvert.DeserializeObject<StructuredOutputVertexAIEs>(text, JsonSerializerSettings);
                    structuredOutputEs = structuredOutputVertexAIEs?.Parse();
                }
                else
                {
                    structuredOutputEs = JsonConvert.DeserializeObject<StructuredOutputEs>(text, JsonSerializerSettings);
                }

                if (structuredOutputEs == null)
                {
                    throw new Exception("StructuredOutputEs is null");
                }

                return new StructuredOutputEsParser(structuredOutputEs).Parse(page);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListing ParseResponse", exception.ToString());
                return (PageType.MayBeListing, null);
            }
        }
    }
}
