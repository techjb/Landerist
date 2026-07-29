using landerist_library.Parsing;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;
using static Google.Cloud.AIPlatform.V1.GenerationConfig.Types;

namespace landerist_library.Infrastructure.Ai;

public sealed record VertexListingParserOptions(
    string CredentialJson,
    string ProjectId,
    string Location,
    string Publisher,
    string Model)
{
    public VertexListingParserOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(CredentialJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(ProjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Location);
        ArgumentException.ThrowIfNullOrWhiteSpace(Publisher);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        return this;
    }
}

public sealed class VertexListingParserClient(
    VertexListingParserOptions options,
    string systemPrompt,
    OpenApiSchema responseSchema,
    IApplicationLogger logger) : IListingParserClient
{
    private readonly VertexListingParserOptions _options =
        options.Validate();

    public LLMProvider Provider => LLMProvider.VertexAI;

    public ListingParserClientResult GetResponse(
        Page page,
        string userInput)
    {
        try
        {
            GenerateContentResponse response = CreateClient()
                .GenerateContent(CreateRequest(page, userInput));
            string? text = response.Candidates
                .SelectMany(candidate =>
                    candidate.Content?.Parts ?? [])
                .Select(part => part.Text)
                .FirstOrDefault(value =>
                    !string.IsNullOrWhiteSpace(value));

            return new ListingParserClientResult(
                text,
                WaitingAIRequest: string.IsNullOrWhiteSpace(text));
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexListingParserClient),
                exception.ToString());
            return new ListingParserClientResult(
                null,
                WaitingAIRequest: true,
                Diagnostic: exception.Message);
        }
    }

    private PredictionServiceClient CreateClient() =>
        new PredictionServiceClientBuilder
        {
            Endpoint = $"{_options.Location}-aiplatform.googleapis.com",
            GoogleCredential = CredentialFactory
                .FromJson<ServiceAccountCredential>(
                    _options.CredentialJson)
                .ToGoogleCredential(),
        }.Build();

    private GenerateContentRequest CreateRequest(
        Page page,
        string text)
    {
        GenerateContentRequest request = new()
        {
            Model =
                $"projects/{_options.ProjectId}"
                + $"/locations/{_options.Location}"
                + $"/publishers/{_options.Publisher}"
                + $"/models/{_options.Model}",
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.2f,
                ResponseMimeType = "application/json",
                ResponseSchema = responseSchema,
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0,
                },
            },
            SystemInstruction = new Content
            {
                Parts =
                {
                    new Part { Text = systemPrompt },
                },
            },
            Labels =
            {
                ["custom_id"] = page.UriHash,
            },
        };
        request.Contents.Add(new Content
        {
            Role = "USER",
            Parts =
            {
                CreateParts(page, text),
            },
        });
        AddSafetySettings(request);
        return request;
    }

    private static IEnumerable<Part> CreateParts(
        Page page,
        string text)
    {
        if (page.ContainsScreenshot())
        {
            return
            [
                new Part
                {
                    InlineData = new Blob
                    {
                        MimeType = "image/png",
                        Data = ByteString.CopyFrom(page.Screenshot),
                    },
                },
                new Part { Text = "Captura de pantalla" },
            ];
        }

        return [new Part { Text = text }];
    }

    private static void AddSafetySettings(
        GenerateContentRequest request)
    {
        foreach (HarmCategory category in new[]
        {
            HarmCategory.HateSpeech,
            HarmCategory.DangerousContent,
            HarmCategory.Harassment,
            HarmCategory.SexuallyExplicit,
            HarmCategory.Unspecified,
        })
        {
            request.SafetySettings.Add(new SafetySetting
            {
                Category = category,
                Threshold = HarmBlockThreshold.Off,
            });
        }
    }
}
