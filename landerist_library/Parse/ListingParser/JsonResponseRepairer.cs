using System.Text;

namespace landerist_library.Parse.ListingParser
{
    internal static class JsonResponseRepairer
    {
        public static string EscapeUnescapedStringValueQuotes(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            var stringBuilder = new StringBuilder(json.Length);
            var inString = false;
            var escaped = false;
            var stringIsValue = false;

            for (var i = 0; i < json.Length; i++)
            {
                var current = json[i];
                if (!inString)
                {
                    if (current == '"')
                    {
                        inString = true;
                        escaped = false;
                        stringIsValue = StartsStringValue(json, i);
                    }

                    stringBuilder.Append(current);
                    continue;
                }

                if (escaped)
                {
                    stringBuilder.Append(current);
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    stringBuilder.Append(current);
                    escaped = true;
                    continue;
                }

                if (current == '"')
                {
                    if (stringIsValue && !IsTerminatingStringValueQuote(json, i))
                    {
                        stringBuilder.Append('\\');
                        stringBuilder.Append(current);
                        continue;
                    }

                    inString = false;
                    stringIsValue = false;
                }

                stringBuilder.Append(current);
            }

            return stringBuilder.ToString();
        }

        private static bool StartsStringValue(string json, int quoteIndex)
        {
            var index = SkipWhitespaceBackwards(json, quoteIndex - 1);
            return index >= 0 && json[index] == ':';
        }

        private static bool IsTerminatingStringValueQuote(string json, int quoteIndex)
        {
            var index = SkipWhitespaceForward(json, quoteIndex + 1);
            if (index >= json.Length)
            {
                return true;
            }

            var next = json[index];
            if (next == '}' || next == ']')
            {
                return true;
            }

            if (next != ',')
            {
                return false;
            }

            index = SkipWhitespaceForward(json, index + 1);
            return index < json.Length &&
                json[index] == '"' &&
                LooksLikePropertyName(json, index);
        }

        private static bool LooksLikePropertyName(string json, int quoteIndex)
        {
            var escaped = false;
            for (var index = quoteIndex + 1; index < json.Length; index++)
            {
                var current = json[index];
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '"')
                {
                    var afterString = SkipWhitespaceForward(json, index + 1);
                    return afterString < json.Length && json[afterString] == ':';
                }
            }

            return false;
        }

        private static int SkipWhitespaceForward(string text, int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return index;
        }

        private static int SkipWhitespaceBackwards(string text, int index)
        {
            while (index >= 0 && char.IsWhiteSpace(text[index]))
            {
                index--;
            }

            return index;
        }
    }
}
