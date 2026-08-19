using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Parsing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace landerist_library.Infrastructure.Ai.LocalAI;

public sealed record LocalAIListingParserOptions(
    string Host,
    int Port = 8000,
    string Model = LocalAIListingParserOptions.DefaultModel,
    int MaxCompletionTokens = 4000,
    int MaxContextWindow = 60000,
    TimeSpan? Timeout = null,
    bool ResolveHost = false)
{
    public const string DefaultModel = "cyankiwi/Qwen3.6-35B-A3B-AWQ-4bit";

    public LocalAIListingParserOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Host);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        if (Port is <= 0 or > 65535) throw new ArgumentOutOfRangeException(nameof(Port));
        if (MaxCompletionTokens <= 0) throw new ArgumentOutOfRangeException(nameof(MaxCompletionTokens));
        if (MaxContextWindow <= 0) throw new ArgumentOutOfRangeException(nameof(MaxContextWindow));
        if (Timeout is { } timeout && timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(Timeout));
        return this;
    }
}

public sealed class LocalAIListingParserClient : IListingParserClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly LocalAIListingParserOptions _options;
    private readonly string _systemPrompt;
    private readonly JsonElement _responseSchema;
    private readonly Func<string, string> _prepareUserInput;
    private readonly IApplicationLogger _logger;
    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;

    public LocalAIListingParserClient(
        LocalAIListingParserOptions options,
        string systemPrompt,
        string responseSchema,
        Func<string, string> prepareUserInput,
        IApplicationLogger logger,
        HttpClient? httpClient = null)
    {
        _options = options.Validate();
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseSchema);
        _systemPrompt = systemPrompt;
        _prepareUserInput = prepareUserInput ?? throw new ArgumentNullException(nameof(prepareUserInput));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _responseSchema = JsonDocument.Parse(responseSchema).RootElement.Clone();
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = _options.Timeout ?? TimeSpan.FromMinutes(10);
        _endpoint = new Uri($"http://{ResolveHost(_options)}:{_options.Port}/v1/chat/completions");
    }

    public LLMProvider Provider => LLMProvider.LocalAI;

    public ListingParserClientResult GetResponse(Page page, string userInput)
    {
        string requestText = _prepareUserInput(userInput);
        try
        {
            string json = JsonSerializer.Serialize(CreateRequest(requestText));
            using StringContent content = new(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = _httpClient
                .PostAsync(_endpoint, content)
                .GetAwaiter()
                .GetResult();
            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{response.StatusCode}: {result}");
            }

            LocalAIResponse? parsed = JsonSerializer.Deserialize<LocalAIResponse>(result, SerializerOptions);
            string? responseText = parsed?.Choices.FirstOrDefault()?.Message.Content;
            string? finishReason = parsed?.Choices.FirstOrDefault()?.FinishReason;
            string usageDiagnostic = FormatUsage(parsed?.Usage);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new ListingParserClientResult(
                    null,
                    true,
                    $"responseText is null or empty. Finish Reason: {finishReason} {usageDiagnostic} InputTokenCount: {page.TokenCount}");
            }

            if (finishReason == "length")
            {
                return new ListingParserClientResult(
                    null,
                    true,
                    $"response was truncated by max_tokens. {usageDiagnostic} InputTokenCount: {page.TokenCount} " +
                    $"ResponseLength: {responseText.Length} ResponseTail: {GetDiagnosticTail(responseText)} Uri: {page.Uri}");
            }

            return new ListingParserClientResult(
                responseText,
                false,
                $"Finish Reason: {finishReason} {usageDiagnostic} InputTokenCount: {page.TokenCount} Uri: {page.Uri}");
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(LocalAIListingParserClient), exception.ToString());
            return new ListingParserClientResult(null, true, exception.Message);
        }
    }

    private object CreateRequest(string text) => new
    {
        model = _options.Model,
        temperature = 0,
        max_tokens = _options.MaxCompletionTokens,
        top_p = 1.0,
        top_k = -1,
        chat_template_kwargs = new { enable_thinking = false },
        messages = new[]
        {
            new { role = "system", content = _systemPrompt },
            new { role = "user", content = text.Replace('"', '”') }
        },
        response_format = new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "esquema_de_respuesta",
                schema = _responseSchema,
                strict = true
            }
        }
    };

    private static string FormatUsage(Usage? usage) => usage == null
        ? "Usage: unavailable."
        : $"PromptTokens: {usage.PromptTokens} CompletionTokens: {usage.CompletionTokens} TotalTokens: {usage.TotalTokens}.";

    private static string GetDiagnosticTail(string responseText)
    {
        const int maximumLength = 2000;
        string tail = responseText.Length <= maximumLength
            ? responseText
            : responseText[^maximumLength..];
        return JsonSerializer.Serialize(tail);
    }

    private string ResolveHost(LocalAIListingParserOptions options)
    {
        if (!options.ResolveHost)
        {
            return options.Host;
        }

        try
        {
            return Dns.GetHostEntry(options.Host).AddressList
                .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork)?
                .ToString() ?? "localhost";
        }
        catch (Exception exception)
        {
            _logger.WriteError($"{nameof(LocalAIListingParserClient)}.{nameof(ResolveHost)}", exception.ToString());
            return "localhost";
        }
    }

    private sealed class LocalAIResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; init; } = [];

        [JsonPropertyName("usage")]
        public Usage? Usage { get; init; }
    }

    private sealed class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; init; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; init; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; init; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; init; }

        [JsonPropertyName("message")]
        public Message Message { get; init; } = new();
    }

    private sealed class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }
}
