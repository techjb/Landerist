using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.UserInput;
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

        private static readonly Dictionary<LLMProvider, IListingParserClient> Clients = new()
        {
            { LLMProvider.OpenAI, new OpenAIListingParserClient() },
            { LLMProvider.VertexAI, new VertexAIListingParserClient() },
            { LLMProvider.LocalAI, new LocalAIListingParserClient() },
        };

        public static (PageType pageType, Listing? listing, bool waitingAIRequest) Parse(Page page)
        {
            page.SetLastParseListing();

            if (Config.BATCH_ENABLED)
            {
                return (PageType.MayBeListing, null, true);
            }

            var text = page.GetListingParserInput();
            if (string.IsNullOrWhiteSpace(text))
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }

            return Clients.TryGetValue(Config.LLM_PROVIDER, out var client)
                ? ParseWithClient(page, text, client)
                : (PageType.ResponseBodyTooShort, null, false);
        }

        public static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseLocalAI(Page page, string text)
        {
            return ParseWithClient(page, text, new LocalAIListingParserClient());
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseWithClient(
            Page page,
            string userInput,
            IListingParserClient client)
        {
            return ParseWithRetry(page, () =>
            {
                var response = client.GetResponse(page, userInput);
                if (response.WaitingAIRequest)
                {
                    WriteLocalAIDiagnostic(client, response.Diagnostic);
                    return (PageType.MayBeListing, null, true);
                }

                var (pageType, listing) = ParseResponse(page, response.ResponseText, client.Provider);
                if (pageType == PageType.MayBeListing)
                {
                    WriteLocalAIDiagnostic(client, "pageType is MayBeListing. " + response.Diagnostic);
                }

                return (pageType, listing, false);
            });
        }

        private static void WriteLocalAIDiagnostic(IListingParserClient client, string? diagnostic)
        {
            if (client.Provider == LLMProvider.LocalAI && !string.IsNullOrWhiteSpace(diagnostic))
            {
                Console.WriteLine("ParseListing ParseLocalAI " + diagnostic);
            }
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
            return ParseResponse(page, text, Config.LLM_PROVIDER);
        }

        private static (PageType pageType, Listing? listing) ParseResponse(Page page, string? text, LLMProvider provider)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (PageType.MayBeListing, null);
            }

            try
            {
                var structuredOutputEs = DeserializeStructuredOutput(text, provider);
                if (structuredOutputEs == null)
                {
                    throw new Exception("StructuredOutputEs is null");
                }

                ListingImageUrlPlaceholders.Resolve(page, structuredOutputEs);
                return new StructuredOutputEsParser(structuredOutputEs).Parse(page);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListing ParseResponse", exception.Message);
                return (PageType.MayBeListing, null);
            }
        }

        private static StructuredOutputEs? DeserializeStructuredOutput(string text, LLMProvider provider)
        {
            if (provider == LLMProvider.VertexAI)
            {
                var structuredOutputVertexAIEs = JsonConvert.DeserializeObject<StructuredOutputVertexAIEs>(text, JsonSerializerSettings);
                return structuredOutputVertexAIEs?.Parse();
            }

            return JsonConvert.DeserializeObject<StructuredOutputEs>(text, JsonSerializerSettings);
        }
    }
}
