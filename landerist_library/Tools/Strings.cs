using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public partial class Strings
    {
        public static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = Replace(text);
            text = BreaklinesToSpace(text);
            text = TabsToSpaces(text);
            text = RemoveUrls(text);
            text = RemoveMultipleDots(text);
            text = RemoveMultipleComas(text);
            text = RemoveMultipleSpaces(text);
            return text.Trim();
        }

        [GeneratedRegex(@"\r\n?|\n")]
        private static partial Regex RegexBreaklines();

        private static string BreaklinesToSpace(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexBreaklines().Replace(text, " ");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();

        public static string RemoveSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexSpace().Replace(text, string.Empty);
        }

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex RegexTabs();

        private static string TabsToSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = text.Replace("\t", " ");
            return RegexTabs().Replace(text, " ");
        }

        [GeneratedRegex(@"(\s*\.)+")]
        private static partial Regex RegexMultipleDots();

        public static string RemoveMultipleDots(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexMultipleDots().Replace(text, ".");
        }

        [GeneratedRegex(@"(\s*,)+")]
        private static partial Regex RegexMultipleComas();

        public static string RemoveMultipleComas(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexMultipleComas().Replace(text, ",");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexMultipleSpaces();

        public static string RemoveMultipleSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexMultipleSpaces().Replace(text, " ");
        }

        [GeneratedRegex(@"http[^\s]+|www\.[^\s]+")]
        private static partial Regex RegexUrl();

        public static string RemoveUrls(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RegexUrl().Replace(text, string.Empty);
        }

        public static string Replace(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text
                .Replace("*", " ")
                .Replace("…", " ")
                .Replace("©", " ")
                .Replace(" :", ":");
        }

        public static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            char[] delimiters = [' ', '\r', '\n'];
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        }

        public static bool IsNumeric(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        public static string GetHash(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static string RemoveHTMLTags(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return HtmlTagsRegex().Replace(input, string.Empty);
        }

        [GeneratedRegex(@"<.*?>")]
        private static partial Regex HtmlTagsRegex();

        public static string SafeJson(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return JsonConvert.SerializeObject(string.Empty);
            }

            return JsonConvert.SerializeObject(input);
        }
    }
}
