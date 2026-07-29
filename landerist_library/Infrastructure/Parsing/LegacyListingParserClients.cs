using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.UserInput;

namespace landerist_library.Infrastructure.Parsing
{
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
