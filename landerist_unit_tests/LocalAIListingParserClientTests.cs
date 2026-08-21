using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Pages;
using System.Net;
using System.Text;
using System.Text.Json;

namespace landerist_unit_tests;

public sealed class LocalAIListingParserClientTests
{
    [Fact]
    public void GetResponse_AcceptsCompleteJsonWhenFinishReasonIsLength()
    {
        LocalAIListingParserClient client = CreateClient(
            """
            {
              "choices": [{"finish_reason":"length","message":{"content":"{\"anuncio\":null}   "}}],
              "usage": {"prompt_tokens":29170,"completion_tokens":4000,"total_tokens":33170}
            }
            """);

        var result = client.GetResponse(CreatePage(), "listing input");

        Assert.False(result.WaitingAIRequest);
        Assert.Equal("{\"anuncio\":null}", result.ResponseText);
        Assert.Contains("complete JSON document", result.Diagnostic);
        Assert.Contains("CompletionTokens: 4000", result.Diagnostic);
    }

    [Fact]
    public void GetResponse_RetriesIncompleteJsonWhenFinishReasonIsLength()
    {
        LocalAIListingParserClient client = CreateClient(
            """
            {
              "choices": [{"finish_reason":"length","message":{"content":"{\"anuncio\":   "}}],
              "usage": {"prompt_tokens":100,"completion_tokens":4000,"total_tokens":4100}
            }
            """);

        var result = client.GetResponse(CreatePage(), "listing input");

        Assert.True(result.WaitingAIRequest);
        Assert.Null(result.ResponseText);
        Assert.Contains("truncated by max_tokens", result.Diagnostic);
        Assert.Contains("ResponseTail: \"{\\u0022anuncio\\u0022:\"", result.Diagnostic);
    }

    [Fact]
    public void GetResponse_SendsFrequencyPenaltyToPreventWhitespaceLoops()
    {
        StubHandler handler = new(
            """
            {"choices":[{"finish_reason":"stop","message":{"content":"{\"anuncio\":null}"}}]}
            """);
        LocalAIListingParserClient client = CreateClient(handler);

        client.GetResponse(CreatePage(), "listing input");

        using JsonDocument request = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(0.2, request.RootElement.GetProperty("frequency_penalty").GetDouble());
    }

    private static LocalAIListingParserClient CreateClient(string responseBody)
    {
        return CreateClient(new StubHandler(responseBody));
    }

    private static LocalAIListingParserClient CreateClient(StubHandler handler)
    {
        HttpClient httpClient = new(handler);
        return new LocalAIListingParserClient(
            new LocalAIListingParserOptions("localhost"),
            "system prompt",
            "{\"type\":\"object\"}",
            text => text,
            new NullLogger(),
            httpClient);
    }

    private static Page CreatePage() => new("https://example.com/listing/1")
    {
        TokenCount = 123
    };

    private sealed class StubHandler(string responseBody) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class NullLogger : IApplicationLogger
    {
        public void WriteError(string source, string message) { }

        public void WriteInfo(string source, string message) { }
    }
}
