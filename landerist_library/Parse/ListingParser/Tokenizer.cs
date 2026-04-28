using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.VertexAI;
using SharpToken;


namespace landerist_library.Parse.ListingParser
{
    public class Tokenizer
    {
        private const string LOCAL_AI_TOKENIZER = "o200k_harmony";

        private const int DEFAULT_MAX_TOKENS = 128000;

        public static int CountSystemTokens()
        {
            var encoding = GptEncoding.GetEncoding(LOCAL_AI_TOKENIZER);
            return encoding.CountTokens(SytemPrompt.Text);
        }

        public static (int, int) CountPageAndSystemTokens(Page page)
        {
            var encoding = GptEncoding.GetEncoding(LOCAL_AI_TOKENIZER);

            string? userInput = page.GetParseListingUserInput();
            int systemTokens = encoding.CountTokens(SytemPrompt.Text);
            int pageTokens = 0;
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                pageTokens = encoding.CountTokens(userInput);
            }
            return (pageTokens, systemTokens);
        }

        public static bool TooManyTokens(Page page)
        {
            var (pageTokens, systemTokens) = CountPageAndSystemTokens(page);
            page.TokenCount = pageTokens;

            int totalTokens = systemTokens + page.TokenCount.Value;
            int maxContextWindow = GetMaxContextWindow();

            return totalTokens > maxContextWindow;
        }

        private static int GetMaxContextWindow()
        {
            var maxContextWindow = DEFAULT_MAX_TOKENS;

            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    maxContextWindow = OpenAIRequest.MAX_CONTEXT_WINDOW;
                    break;
                case LLMProvider.VertexAI:
                    maxContextWindow = VertexAIRequest.MAX_CONTEXT_WINDOW;
                    break;
                case LLMProvider.LocalAI:
                    maxContextWindow = LocalAIRequest.MAX_CONTEXT_WINDOW;
                    break;
            }

            if (maxContextWindow <= 0)
            {
                maxContextWindow = DEFAULT_MAX_TOKENS;
            }
            return maxContextWindow;
        }
    }
}
