using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public class BreakLines
    {
        public static string ToSpace(string text) 
        {
            return Regex.Replace(text, @"\r\n?|\n", " ");
        }
    }
}
