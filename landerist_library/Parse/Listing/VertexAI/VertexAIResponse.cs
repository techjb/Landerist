using Google.Cloud.AIPlatform.V1;
using landerist_library.Tools;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIResponse
    {
        public static (string?, string?) GetFunctionNameAndArguments(GenerateContentResponse response)
        {
            try
            {
                if (response.Candidates != null &&
                    response.Candidates[0].Content != null &&
                    response.Candidates[0].Content.Parts != null)
                {
                    var part = response.Candidates[0].Content.Parts[0];
                    if (part.FunctionCall == null)
                    {
                        return GetFunctionNameAndArgumentsWithText(response);
                    }
                    var name = part.FunctionCall.Name;
                    var args = part.FunctionCall.Args;
                    if (name != null && args != null)
                    {
                        return (name, args.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("VertexAIRequest GetFunctionNameAndArguments", response.ToString(), exception);
            }
            return (null, null);
        }

        private static (string?, string?) GetFunctionNameAndArgumentsWithText(GenerateContentResponse response)
        {
            try
            {
                var text = response.Candidates[0].Content.Parts[0].Text;
                string searckKey = "print(default_api.";
                if (!text.Contains(searckKey))
                {
                    return (null, null);
                }
                text = text[(text.IndexOf(searckKey) + searckKey.Length)..];
                if (!text.Contains('('))
                {
                    return (null, null);
                }
                string functionName = text[..text.IndexOf('(')];
                string args = text[(text.IndexOf('(') + 1)..];
                if (!args.Contains("))"))
                {
                    return (null, null);
                }
                args = args[..(args.IndexOf("))"))];

                string argsJson = ConvertToJSON(args);
                return (functionName, argsJson);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("VertexAIRequest GetFunctionNameAndArgumentsWithText", exception);
            }

            return (null, null);
        }

        private static string ConvertToJSON(string input)
        {
            string pattern = @"(\w+)=('.*?'|[^,]+)";
            var matches = Regex.Matches(input, pattern);

            var keyValuePairs = new List<string>();
            foreach (Match match in matches.Cast<Match>())
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                value = Strings.RemoveHTMLTags(value);

                if (value.StartsWith('\'') && value.EndsWith('\''))
                {
                    value = Strings.SafeJson(value.Trim('\''));
                }
                else if (bool.TryParse(value, out bool boolValue))
                {
                    value = boolValue.ToString().ToLower();
                }
                else if (value.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    value = "null";
                }
                keyValuePairs.Add($"\"{key}\": {value}");
            }

            return "{" + string.Join(", ", keyValuePairs) + "}";
        }

    }
}
