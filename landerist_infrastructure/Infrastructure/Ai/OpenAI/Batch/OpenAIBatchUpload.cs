using landerist_library.Pages;
using OpenAI;
using System.Text.Json;

namespace landerist_library.Infrastructure.Ai.OpenAI.Batch;

public sealed class OpenAIBatchUpload(
    OpenAIBatchOptions options,
    string systemPrompt,
    string responseSchema)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    private readonly OpenAIBatchOptions _options = options.Validate();
    private readonly string _systemPrompt =
        !string.IsNullOrWhiteSpace(systemPrompt)
            ? systemPrompt
            : throw new ArgumentException("System prompt is required.", nameof(systemPrompt));
    private readonly JsonSchema _responseSchema =
        !string.IsNullOrWhiteSpace(responseSchema)
            ? new JsonSchema("esquema_de_respuesta", responseSchema)
            : throw new ArgumentException("Response schema is required.", nameof(responseSchema));

    public string Serialize(Page page, string userInput)
    {
        StructuredRequestData request = new()
        {
            custom_id = page.UriHash,
            method = "POST",
            url = "/v1/chat/completions",
            body = new StructuredBody
            {
                model = _options.Model,
                temperature = 0,
                messages =
                [
                    new BatchMessage { role = "system", content = _systemPrompt },
                    new BatchMessage { role = "user", content = userInput }
                ],
                response_format = new StructuredResponseFormat
                {
                    type = "json_schema",
                    json_schema = _responseSchema
                }
            }
        };

        return JsonSerializer.Serialize(request, SerializerOptions);
    }
}
