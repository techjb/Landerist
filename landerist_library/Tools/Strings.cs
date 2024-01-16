using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public partial class Strings
    {
        public static string Clean(string text)
        {
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
            return RegexBreaklines().Replace(text, " ");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();

        public static string RemoveSpaces(string text)
        {
            return RegexSpace().Replace(text, string.Empty);
        }

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex RegexTabs();
        private static string TabsToSpaces(string text)
        {
            text = text.Replace("\t", " ");            
            return RegexTabs().Replace(text, " ");
        }

        [GeneratedRegex(@"(\s*\.)+")]
        private static partial Regex RegexMultipleDots();
        public static string RemoveMultipleDots(string text)
        {
            return RegexMultipleDots().Replace(text, ".");
        }

        [GeneratedRegex(@"(\s*,)+")]
        private static partial Regex RegexMultipleComas();
        public static string RemoveMultipleComas(string text)
        {
            return RegexMultipleComas().Replace(text, ",");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexMultipleSpaces();
        public static string RemoveMultipleSpaces(string text)
        {
            return RegexMultipleSpaces().Replace(text, " ");
        }

        [GeneratedRegex(@"http[^\s]+|www\.[^\s]+")]
        private static partial Regex RegexUrl();
        public static string RemoveUrls(string text)
        {
            return RegexUrl().Replace(text, string.Empty);
        }
        public static string Replace(string text)
        {
            return text
                .Replace("*", " ")
                .Replace("…", " ")
                .Replace("©", " ")
                .Replace(" :", ":")
                ;
        }

        public static int CountWords(string text)
        {
            char[] delimiters = [' ', '\r', '\n'];
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static bool IsNumeric(string text)
        {
            return int.TryParse(text, out _);
        }

        public static string GetHash(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }

    }
}
