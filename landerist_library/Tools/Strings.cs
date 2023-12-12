using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public class Strings
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

        private static string BreaklinesToSpace(string text)
        {
            return Regex.Replace(text, @"\r\n?|\n", " ");
        }

        public static string RemoveSpaces(string text)
        {
            return Regex.Replace(text, @"\s+", string.Empty);
        }

        private static string TabsToSpaces(string text)
        {
            text = text.Replace("\t", " ");
            const string reduceMultiSpace = @"[ ]{2,}";
            return Regex.Replace(text, reduceMultiSpace, " ");
        }

        public static string RemoveMultipleDots(string text)
        {
            return Regex.Replace(text, @"(\s*\.)+", ".");
        }

        public static string RemoveMultipleComas(string text)
        {
            return Regex.Replace(text, @"(\s*,)+", ",");
        }

        public static string RemoveMultipleSpaces(string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        public static string RemoveUrls(string text)
        {
            return Regex.Replace(text, @"http[^\s]+|www\.[^\s]+", string.Empty);
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
    }
}
