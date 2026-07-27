using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.UserInput;
using landerist_library.Parse.ListingParser.VertexAI;

namespace landerist_library.Parse.ListingParser
{
    public sealed record ListingParserClientResult(string? ResponseText, bool WaitingAIRequest, string? Diagnostic = null);

    public interface IListingParserClient
    {
        LLMProvider Provider { get; }

        ListingParserClientResult GetResponse(Page page, string userInput);
    }

    public sealed class OpenAIListingParserClient : IListingParserClient
    {
        public LLMProvider Provider => LLMProvider.OpenAI;

        public ListingParserClientResult GetResponse(Page page, string userInput)
        {
            var response = OpenAIRequest.GetChatResponse(userInput);
            return response?.FirstChoice == null
                ? new ListingParserClientResult(null, true)
                : new ListingParserClientResult(response.FirstChoice, false);
        }
    }

    public sealed class VertexAIListingParserClient : IListingParserClient
    {
        public LLMProvider Provider => LLMProvider.VertexAI;

        public ListingParserClientResult GetResponse(Page page, string userInput)
        {
            var generateContentResponse = VertexAIRequest.GetResponse(page, userInput).GetAwaiter().GetResult();
            if (generateContentResponse == null)
            {
                return new ListingParserClientResult(null, true);
            }

            return new ListingParserClientResult(VertexAIResponse.GetResponseText(generateContentResponse), false);
        }
    }

    public sealed class LocalAIListingParserClient : IListingParserClient
    {
        public LLMProvider Provider => LLMProvider.LocalAI;

        public ListingParserClientResult GetResponse(Page page, string userInput)
        {
            var requestText = ListingImageUrlPlaceholders.ReplaceImageUrls(userInput);
            var response = new LocalAIRequest().GetResponse(requestText).GetAwaiter().GetResult();
            if (response == null)
            {
                return new ListingParserClientResult(null, true, "response is null.");
            }

            var finishReason = response.GetFinishReason();
            var responseText = response.GetResponseText();
            if (string.IsNullOrWhiteSpace(responseText))
            {
                var diagnostic = "responseText is null or empty. Finish Reason: " + finishReason + " TokenCount: " + page.TokenCount;
                return new ListingParserClientResult(null, true, diagnostic);
            }

            if (finishReason == "length")
            {
                var diagnostic = "response was truncated by max_tokens. TokenCount: " + page.TokenCount + " Uri: " + page.Uri;
                return new ListingParserClientResult(null, true, diagnostic);
            }

            var successDiagnostic = "Finish Reason: " + finishReason + " TokenCount " + page.TokenCount + " Uri: " + page.Uri;
            return new ListingParserClientResult(responseText, false, successDiagnostic);
        }
    }
}
