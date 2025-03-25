using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser.Anthropic
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
            List<SystemMessage> systemMessages = [new SystemMessage(SystemPrompt)];
            var tools = new AnthropicTools().GetTools();

            var parameters = new MessageParameters()
            {
                Messages = [message],
                Model = ANTHROPIC_MODEL,
                Stream = false,
                System = systemMessages,
                Temperature = 0m,
                MaxTokens = MAX_TOKENS,
                Tools = tools
            };

            try
            {
                return Task.Run(async () => await client.Messages.GetClaudeMessageAsync(parameters)).Result;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("AntropicRequest GetResponse", exception);
            }

            return null;
        }

        private static Message GetMessage(Page page, string text)
        {
            if (page.ContainsScreenshot())
            {
                return new Message()
                {
                    Role = RoleType.User,
                    Content =
                    [
                        new ImageContent()
                        {
                            Source = new ImageSource()
                            {
                                MediaType = "image/" + Config.SCREENSHOT_TYPE.ToString().ToLower(),
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
