using Anthropic.SDK.Messaging;

namespace landerist_library.Parse.Listing.Anthropic
{
    public class AnthropicResponse
    {
        public static (string?, string?) GetFunctionNameAndArguments(MessageResponse messageResponse)
        {
            try
            {
                var toolUse = messageResponse.Content.OfType<ToolUseContent>().First();
                return (toolUse.Name, toolUse.Input.ToJsonString());
            }
            catch
            {
            }
            return (null, null);
        }
    }
}
