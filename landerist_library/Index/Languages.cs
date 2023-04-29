namespace landerist_library.Index
{
    internal class Languages
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

        public static bool ContainsNotAllowedES(Uri uri)
        {
            return ContainsNotAllowed(uri, "es");
        }
        public static bool ContainsNotAllowed(Uri uri, string allowedLanguage)
        {
            string absolutePath = uri.AbsolutePath;
            string[] pathComponents = absolutePath.Split('/');

            foreach (string path in pathComponents)
            {
                if (path.Equals(string.Empty))
                {
                    continue;
                }
                if (ContainsNotAllowed(path, allowedLanguage))
                {
                    return true;
                }              
            }
            return false;
        }

        protected static bool ContainsNotAllowed(string path, string allowedLanguage)
        {
            string[] pathItems = new string[] { path };
            if (path.Contains('-'))
            {
                pathItems = path.Split('-');
            }

            if (pathItems.Length > 3)
            {
                return false;
            }

            foreach (var pathItem in pathItems)
            {
                if (string.Equals(pathItem, allowedLanguage, StringComparison.OrdinalIgnoreCase))
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
    }
}
