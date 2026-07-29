using landerist_library.Application.Logging;
using landerist_domain.Parsing.Materialization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_library.Websites;
using landerist_library.Pages;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.UserInput;
using landerist_library.Application.Statistics;
using landerist_library.Application.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;

namespace landerist_library.Infrastructure.Parsing
{
    public class ParseListing
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
        };

        private readonly ListingParserOrchestrationOptions Options;
        private readonly ListingParserClientCatalog Clients;
        private readonly ListingParsingServices ParsingServices;
        private readonly StructuredOutputMaterializationOperations MaterializationOperations;
        private readonly IApplicationLogger Logger;

        public ParseListing(
            ListingParserOrchestrationOptions options,
            ListingParserClientCatalog clients,
            ListingParsingServices parsingServices,
            StructuredOutputMaterializationOperations materializationOperations,
            IApplicationLogger logger)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(clients);
            ArgumentNullException.ThrowIfNull(parsingServices);
            Options = options;
            Clients = clients;
            ParsingServices = parsingServices;
            MaterializationOperations = materializationOperations ?? throw new ArgumentNullException(nameof(materializationOperations));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public (PageType pageType, Listing? listing, bool waitingAIRequest) Parse(Page page, HostStatistics statistics)
        {
            page.SetLastParseListing();

            if (Options.BatchEnabled)
            {
                return (PageType.MayBeListing, null, true);
            }

            var text = page.ListingParserInput;
            if (string.IsNullOrWhiteSpace(text))
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }

            return Clients.TryGet(Options.Provider, out IListingParserClient? client)
                ? ParseWithClient(page, text, client, statistics, ParsingServices)
                : (PageType.ResponseBodyTooShort, null, false);
        }

        public (PageType pageType, Listing? listing, bool waitingAIRequest) ParseLocalAI(Page page, string text, HostStatistics statistics)
        {
            return Clients.TryGet(LLMProvider.LocalAI, out IListingParserClient? client)
                ? ParseWithClient(page, text, client, statistics, ParsingServices)
                : (PageType.ResponseBodyTooShort, null, false);
        }

        private (PageType pageType, Listing? listing, bool waitingAIRequest) ParseWithClient(
            Page page,
            string userInput,
            IListingParserClient client,
            HostStatistics statistics,
            ListingParsingServices parsingServices)
        {
            return ParseWithRetry(page, () =>
            {
                var response = client.GetResponse(page, userInput);
                if (response.WaitingAIRequest)
                {
                    WriteLocalAIDiagnostic(client, response.Diagnostic);
                    return (PageType.MayBeListing, null, true);
                }

                var (pageType, listing) = ParseResponse(page, response.ResponseText, client.Provider, parsingServices);
                if (pageType == PageType.MayBeListing)
                {
                    WriteLocalAIDiagnostic(client, "pageType is MayBeListing. " + response.Diagnostic);
                }

                return (pageType, listing, false);
            }, statistics);
        }

        private void WriteLocalAIDiagnostic(IListingParserClient client, string? diagnostic)
        {
            if (client.Provider == LLMProvider.LocalAI && !string.IsNullOrWhiteSpace(diagnostic))
            {
                Logger.WriteInfo(nameof(ParseListing), diagnostic);
            }
        }

        private static (PageType pageType, Listing? listing, bool waitingAIRequest) ParseWithRetry(
            Page page,
            Func<(PageType pageType, Listing? listing, bool waitingAIRequest)> parse,
            HostStatistics statistics)
        {
            var result = parse();
            if (!ShouldRetryNotListing(page, result.pageType))
            {
                return result;
            }

            statistics.InsertDailyCounter(page.Website.Host, HostStatisticsKey.ParseListingRetryNotListing);
            return parse();
        }

        private static bool ShouldRetryNotListing(Page page, PageType pageType)
        {
            return pageType == PageType.NotListingByParser
                && page.Website.MatchesListingUrlRegex(page.Uri);
        }

        public (PageType pageType, Listing? listing) ParseResponse(Page page, string? text, LLMProvider? provider = null)
        {
            return ParseResponse(page, text, provider ?? Options.Provider, ParsingServices);
        }

        private (PageType pageType, Listing? listing) ParseResponse(Page page, string? text, LLMProvider provider, ListingParsingServices parsingServices)
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
                return new StructuredOutputEsParser(
                    structuredOutputEs,
                    parsingServices.MaterializationRules,
                    parsingServices.TimeProvider,
                    MaterializationOperations)
                    .Parse(page, parsingServices.WebsiteAccess);
            }
            catch (Exception exception)
            {
                Logger.WriteError("ParseListing.ParseResponse", exception.ToString());
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
