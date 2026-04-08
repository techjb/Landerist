using landerist_library.Websites;

namespace landerist_library.Index
{
    internal class LanguageValidator
    {
        private static readonly HashSet<string> Iso6391Codes = new(StringComparer.OrdinalIgnoreCase)
        {
            "aa", "ab", "af", "ak", "sq", "am", "ar", "an", "hy", "as",
            "av", "ae", "ay", "az", "ba", "bm", "eu", "be", "bn", "bh",
            "bi", "bs", "br", "bg", "my", "ca", "ch", "ce", "ny", "zh",
            "cv", "kw", "co", "cr", "hr", "cs", "da", "dv", "nl", "dz",
            "en", "eo", "et", "ee", "fo", "fj", "fi", "fr", "ff", "gl",
            "ka", "de", "el", "gn", "gu", "ht", "ha", "he", "hz", "hi",
            "ho", "hu", "ia", "id", "ie", "ga", "ig", "ik", "io", "is",
            "it", "iu", "ja", "jv", "kl", "kn", "kr", "ks", "kk", "km",
            "ki", "rw", "ky", "kv", "kg", "ko", "ku", "kj", "la", "lb",
            "lg", "li", "ln", "lo", "lt", "lu", "lv", "gv", "mk", "mg",
            "ms", "ml", "mt", "mi", "mr", "mh", "mn", "na", "nv", "nd",
            "ne", "ng", "nb", "nn", "no", "ii", "nr", "oc", "oj", "cu",
            "om", "or", "os", "pa", "pi", "fa", "pl", "ps", "pt", "qu",
            "rm", "rn", "ro", "ru", "sa", "sc", "sd", "se", "sm", "sg",
            "sr", "gd", "sn", "si", "sk", "sl", "so", "st", "es", "su",
            "sw", "ss", "sv", "ta", "te", "tg", "th", "ti", "bo", "tk",
            "tl", "tn", "to", "tr", "ts", "tt", "tw", "ty", "ug", "uk",
            "ur", "uz", "ve", "vi", "vo", "wa", "cy", "wo", "fy", "xh",
            "yi", "yo", "za", "zu"
        };

        public static bool ContainsNotAllowed(Uri uri, LanguageCode allowedLanguage)
        {
            if (uri is null)
            {
                return false;
            }

            string absolutePath = uri.AbsolutePath;
            string[] pathComponents = absolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathComponents.Length == 0)
            {
                return false;
            }

            var firstSegment = pathComponents[0];
            return ContainsNotAllowed(firstSegment, allowedLanguage);
        }

        protected static bool ContainsNotAllowed(string path, LanguageCode allowedLanguage)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string[] pathItems = path.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (pathItems.Length == 0 || pathItems.Length > 3)
            {
                return false;
            }

            foreach (var pathItem in pathItems)
            {
                if (string.Equals(pathItem, allowedLanguage.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Iso6391Codes.Contains(pathItem))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidLanguageAndCountry(Website website, string hreflang)
        {
            if (website is null || string.IsNullOrWhiteSpace(hreflang))
            {
                return false;
            }

            var hrefLangParts = hreflang.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (hrefLangParts.Length == 0 || hrefLangParts.Length > 2)
            {
                return false;
            }

            var language = hrefLangParts[0];
            var country = hrefLangParts.Length == 2 ? hrefLangParts[1] : string.Empty;

            if (!string.Equals(language, website.LanguageCode.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(country) &&
                !string.Equals(country, website.CountryCode.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
