namespace landerist_library.Tools
{
    public static class Uris
    {
        public static Uri CleanUri(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            string cleanedQuery = CleanQueryString(uri.Query);
            UriBuilder builder = new(uri)
            {
                Query = cleanedQuery
            };

            return builder.Uri;
        }

        private static string CleanQueryString(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return string.Empty;
            }

            if (query.StartsWith('?'))
            {
                query = query[1..];
            }

            if (query.Length == 0)
            {
                return string.Empty;
            }

            List<string> orderedKeys = [];
            Dictionary<string, string?> keyedParameters = [];
            HashSet<string> flagParameters = [];

            foreach (string parameter in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int separatorIndex = parameter.IndexOf('=');

                if (separatorIndex < 0)
                {
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        flagParameters.Add(parameter);
                    }

                    continue;
                }

                string key = parameter[..separatorIndex];
                string value = parameter[(separatorIndex + 1)..];

                if (!keyedParameters.ContainsKey(key))
                {
                    orderedKeys.Add(key);
                }

                keyedParameters[key] = value;
            }

            List<string> parts = [];

            foreach (string key in orderedKeys)
            {
                parts.Add($"{key}={keyedParameters[key]}");
            }

            parts.AddRange(flagParameters);

            return string.Join("&", parts);
        }
    }
}
