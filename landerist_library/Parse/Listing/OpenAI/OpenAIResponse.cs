using OpenAI.Chat;

namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIResponse
    {
        public static (string?, string?) GetFunctionNameAndArguments(ChatResponse chatResponse)
        {
            try
            {
                return (chatResponse.FirstChoice.Message.ToolCalls[0].Function.Name,
                    chatResponse.FirstChoice.Message.ToolCalls[0].Function.Arguments.ToString());
            }
            catch
            {
            }
            return (null, null);
        }
    }
}
