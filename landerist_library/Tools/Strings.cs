using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public class Strings
    {
        public static string BreaklinesToSpace(string text) 
        {
            return Regex.Replace(text, @"\r\n?|\n", " ");
        }

        public static string RemoveBreaklines(string text)
        {
            return Regex.Replace(text, @"\r\n?|\n", string.Empty);
        }

        public static string RemoveSpaces(string text)
        {
            return Regex.Replace(text, @"\s+", string.Empty);
        }
    }
}
