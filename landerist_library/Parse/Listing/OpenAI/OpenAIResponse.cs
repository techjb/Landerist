using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
