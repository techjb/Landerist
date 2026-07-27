using System.Text.Json;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public Dictionary<string, string> GetHttpRequestHeaders()
        {
            var value = NullIfWhiteSpace(HttpRequestHeaders);
            if (value == null)
            {
                return [];
            }

            if (TryParseJsonHttpRequestHeaders(value, out var jsonHeaders))
            {
                return jsonHeaders;
            }

            return ParseLineHttpRequestHeaders(value);
        }

        public HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, Uri uri)
        {
            HttpRequestMessage request = new(method, uri);
            request.Headers.UserAgent.ParseAdd(BrowserUserAgent);
            ApplyHttpRequestHeaders(request);
            return request;
        }

        public void ApplyHttpRequestHeaders(HttpRequestMessage request)
        {
            ApplyHttpRequestHeaders(request, GetHttpRequestHeaders());
        }

        public static void ApplyHttpRequestHeaders(
            HttpRequestMessage request,
            IReadOnlyDictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                if (string.Equals(header.Key, "User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private static bool TryParseJsonHttpRequestHeaders(
            string value,
            out Dictionary<string, string> headers)
        {
            headers = new(StringComparer.OrdinalIgnoreCase);

            var json = value.Trim();
            if (!json.StartsWith('{') && json.StartsWith('"') && json.Contains(':'))
            {
                json = "{" + json + "}";
            }

            if (!json.StartsWith('{'))
            {
                return false;
            }

            try
            {
                var parsedHeaders = JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
                if (parsedHeaders == null)
                {
                    return false;
                }

                foreach (var header in parsedHeaders)
                {
                    var headerName = NullIfWhiteSpace(header.Key);
                    var headerValue = NullIfWhiteSpace(header.Value);
                    if (headerName != null && headerValue != null)
                    {
                        headers[headerName] = headerValue;
                    }
                }

                return true;
            }
            catch (JsonException)
            {
                headers.Clear();
                return false;
            }
        }

        private static Dictionary<string, string> ParseLineHttpRequestHeaders(string value)
        {
            Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
            var lines = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var index = line.IndexOf(':');
                if (index <= 0)
                {
                    continue;
                }

                var headerName = NullIfWhiteSpace(line[..index].Trim().Trim('"'));
                var headerValue = NullIfWhiteSpace(line[(index + 1)..].Trim().TrimEnd(',').Trim().Trim('"'));
                if (headerName != null && headerValue != null)
                {
                    headers[headerName] = headerValue;
                }
            }

            return headers;
        }
    }
}
