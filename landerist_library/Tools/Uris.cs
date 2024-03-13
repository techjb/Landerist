namespace landerist_library.Tools
{
    public class Uris
    {

        public static Uri CleanUri(Uri uri)
        {
            string cleanedQuery = CleanQueryString(uri.Query);
            UriBuilder builder = new(uri)
            {
                Query = cleanedQuery
            };
            return builder.Uri;
        }

        private static string CleanQueryString(string query)
        {
            if (query.StartsWith('?'))
            {
                query = query[1..];
            }

            string[] parameters = query.Split('&');
            Dictionary<string, string> dictionary = [];
            HashSet<string> hashSet = [];
            foreach (string parameter in parameters)
            {
                string[] keyValue = parameter.Split('=');
                if (keyValue.Length == 1)
                {
                    hashSet.Add(keyValue[0]);
                }
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = keyValue[1];
                    dictionary[key] = value;
                }
            }

            string newQuery = string.Join("&", dictionary.Select(p => $"{p.Key}={p.Value}"));
            newQuery += string.Join("&", hashSet.ToList());
            return newQuery;
        }
    }
}
