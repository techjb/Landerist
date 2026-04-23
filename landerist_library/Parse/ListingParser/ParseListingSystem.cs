using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.VertexAI;
using SharpToken;

namespace landerist_library.Parse.ListingParser
{
    public class ParseListingSystem
    {
        public static readonly string SystemPrompt =
            "Tu tarea es analizar el código HTML proporcionado por el usuario y determinar si corresponde a una página web de un único anuncio inmobiliario. " +

        private const int DEFAULT_MAX_TOKENS = 128000;

        public static bool TooManyTokens(Page page)
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

            return TooManyTokens(page, maxContextWindow);
        }

        public static bool TooManyTokens(Page page, int maxContextWindow)
        {
            if (maxContextWindow <= 0)
            {
                maxContextWindow = DEFAULT_MAX_TOKENS;
            }

            var encoding = GptEncoding.GetEncoding(Config.LOCAL_AI_TOKENIZER);

            int systemTokens = encoding.CountTokens(SystemPrompt);
            string? userInput = page.GetParseListingUserInput();
            if (string.IsNullOrWhiteSpace(userInput))
            {
                page.TokenCount = 0;
                return false;
            }

            page.TokenCount = encoding.CountTokens(userInput);
            int totalTokens = systemTokens + page.TokenCount.Value;

            return totalTokens > maxContextWindow;
        }

        public static string GetSystemPrompt()
        {
            return SystemPrompt;
        }
    }
}
