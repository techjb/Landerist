using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Parsing;
using OpenAI;
using OpenAI.Chat;

namespace landerist_library.Infrastructure.Ai.OpenAI;

public sealed record OpenAIListingParserOptions(
    string ApiKey,
    string Model = OpenAIListingParserOptions.DefaultModel)
{
    public const string DefaultModel = "gpt-5-mini-2025-08-07";
    public const int DefaultMaxContextWindow = 128000;

    public OpenAIListingParserOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        return this;
    }
}

public sealed class OpenAIListingParserClient : IListingParserClient
{
    private readonly OpenAIListingParserOptions _options;
    private readonly string _systemPrompt;
    private readonly global::OpenAI.JsonSchema _responseSchema;
    private readonly IApplicationLogger _logger;
    private readonly OpenAIClient _client;

    public OpenAIListingParserClient(
        OpenAIListingParserOptions options,
        string systemPrompt,
        string responseSchema,
        IApplicationLogger logger)
    {
        _options = options.Validate();
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseSchema);
        _systemPrompt = systemPrompt;
        _responseSchema = new global::OpenAI.JsonSchema("esquema_de_respuesta", responseSchema);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = new OpenAIClient(_options.ApiKey);
    }

    public LLMProvider Provider => LLMProvider.OpenAI;

    public ListingParserClientResult GetResponse(Page page, string userInput)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);

        ChatRequest request = new(
            messages:
            [
                new Message(Role.System, _systemPrompt),
                new Message(Role.User, userInput),
            ],
            model: _options.Model,
            responseFormat: TextResponseFormat.JsonSchema,
            jsonSchema: _responseSchema);

        try
        {
            ChatResponse response = _client.ChatEndpoint
                .GetCompletionAsync(request)
                .GetAwaiter()
                .GetResult();

            return response.FirstChoice is null
                ? new ListingParserClientResult(null, true)
                : new ListingParserClientResult(response.FirstChoice, false);
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(OpenAIListingParserClient), exception.ToString());
            return new ListingParserClientResult(null, true, exception.Message);
        }
    }
}
