using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using Newtonsoft.Json;
using static Google.Cloud.AIPlatform.V1.GenerationConfig.Types;

namespace landerist_library.Infrastructure.Ai;

public sealed record VertexAddressSelectorOptions(
    string CredentialJson,
    string ProjectId,
    string Location,
    string Publisher,
    string Model)
{
    public VertexAddressSelectorOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(CredentialJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(ProjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Location);
        ArgumentException.ThrowIfNullOrWhiteSpace(Publisher);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        return this;
    }
}

public sealed class VertexAddressCandidateSelector(
    VertexAddressSelectorOptions options,
    IApplicationLogger logger) : IAddressCandidateSelector
{
    private const string SystemPrompt =
        """
        Encuentra en la lista la dirección postal equivalente a la dirección
        buscada. Acepta diferencias menores de mayúsculas o espacios e ignora
        datos internos de la finca, pero no diferencias de calle, número o
        localidad. Devuelve JSON con la propiedad "equivalente", usando null
        cuando no exista una coincidencia.
        """;

    private readonly VertexAddressSelectorOptions _options =
        options.Validate();

    public string? Select(
        string searchAddress,
        IReadOnlyList<string> candidates)
    {
        if (string.IsNullOrWhiteSpace(searchAddress)
            || candidates.Count == 0)
        {
            return null;
        }

        try
        {
            GenerateContentResponse response = CreateClient()
                .GenerateContent(CreateRequest(searchAddress, candidates));
            string? text = response.Candidates
                .SelectMany(candidate =>
                    candidate.Content?.Parts ?? [])
                .Select(part => part.Text)
                .FirstOrDefault(value =>
                    !string.IsNullOrWhiteSpace(value));
            return string.IsNullOrWhiteSpace(text)
                ? null
                : JsonConvert.DeserializeObject<Selection>(text)
                    ?.Equivalent;
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexAddressCandidateSelector),
                exception.ToString());
            return null;
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
        string searchAddress,
        IReadOnlyList<string> candidates) =>
        new()
        {
            Model =
                $"projects/{_options.ProjectId}"
                + $"/locations/{_options.Location}"
                + $"/publishers/{_options.Publisher}"
                + $"/models/{_options.Model}",
            Contents =
            {
                new Content
                {
                    Role = "USER",
                    Parts =
                    {
                        new Part
                        {
                            Text =
                                $"Dirección buscada:\n{searchAddress}\n\n"
                                + "Direcciones candidatas:\n"
                                + string.Join('\n', candidates),
                        },
                    },
                },
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.2f,
                ResponseMimeType = "application/json",
                ResponseSchema = ResponseSchema,
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0,
                },
            },
            SystemInstruction = new Content
            {
                Parts =
                {
                    new Part { Text = SystemPrompt },
                },
            },
        };

    private static readonly OpenApiSchema ResponseSchema = new()
    {
        Type = Google.Cloud.AIPlatform.V1.Type.Object,
        Properties =
        {
            ["equivalente"] = new OpenApiSchema
            {
                Nullable = true,
                Type = Google.Cloud.AIPlatform.V1.Type.String,
            },
        },
    };

    private sealed class Selection
    {
        [JsonProperty("equivalente")]
        public string? Equivalent { get; init; }
    }
}
