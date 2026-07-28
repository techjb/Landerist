using System.Text.RegularExpressions;

namespace landerist_library.Infrastructure.Html;

internal static partial class HtmlTextNormalizer
{
    public static string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text = text
            .Replace("*", " ")
            .Replace("…", " ")
            .Replace("©", " ")
            .Replace(" :", ":");
        text = Breaklines().Replace(text, " ");
        text = text.Replace("\t", " ");
        text = Tabs().Replace(text, " ");
        text = Urls().Replace(text, string.Empty);
        text = MultipleDots().Replace(text, ".");
        text = MultipleCommas().Replace(text, ",");
        text = MultipleSpaces().Replace(text, " ");
        return text.Trim();
    }

    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        char[] delimiters = [' ', '\r', '\n'];
        return text.Split(
            delimiters,
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries).Length;
    }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex Breaklines();

    [GeneratedRegex(@"[ ]{2,}")]
    private static partial Regex Tabs();

    [GeneratedRegex(@"http[^\s]+|www\.[^\s]+")]
    private static partial Regex Urls();

    [GeneratedRegex(@"(\s*\.)+")]
    private static partial Regex MultipleDots();

    [GeneratedRegex(@"(\s*,)+")]
    private static partial Regex MultipleCommas();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpaces();
}
