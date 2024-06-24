using landerist_library.Websites;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Anthropic.SDK;
using landerist_library.Configuration;

namespace landerist_library.Parse.Listing.Anthropic
{
    public class AnthropicRequest : ParseListingRequest
    {
        public const string ANTHROPIC_MODEL = AnthropicModels.Claude3Haiku;

        public const int MAX_CONTEXT_WINDOW = 128000;

        //https://docs.anthropic.com/en/docs/about-claude/models
        public const int MAX_TOKENS = 4096; // for haiku
        public static bool TooManyTokens(Page page)
        {
            return TooManyTokens(page, MAX_CONTEXT_WINDOW);
        }

        public static MessageResponse? GetResponse(Page page, string text)
        {
            var apiAutentication = new APIAuthentication(PrivateConfig.ANTHROPIC_API_KEY);
            var client = new AnthropicClient(apiAutentication);
            var message = GetMessage(page, text);

            var parameters = new MessageParameters()
            {
                Messages = [message],
                Model = ANTHROPIC_MODEL,
                Stream = false,
                SystemMessage = SystemPrompt,
                Temperature = 0m,
                MaxTokens = MAX_TOKENS,
            };

            var tools = AnthropicTools.GetTools();
            try
            {
                return Task.Run(async () => await client.Messages.GetClaudeMessageAsync(parameters, tools)).Result;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("AntropicRequest GetResponse", exception);
            }

            return null;
        }

        private static Message GetMessage(Page page, string text)
        {
            if (page.ContainsScreenshot())
            {
                // todo: ensure that images are < 5MB
                return new Message()
                {
                    Role = RoleType.User,
                    Content =
                    [
                        new ImageContent()
                        {
                            Source = new ImageSource()
                            {
                                MediaType = "image/png",
                                Data = Convert.ToBase64String(page.Screenshot!)
                            }
                        }
                    ]
                };
            }
            return new(RoleType.User, text);
        }

    }
}
