using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Pages;
using System.Net;
using System.Text;

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
              "choices": [{"finish_reason":"length","message":{"content":"{\"anuncio\":"}}],
              "usage": {"prompt_tokens":100,"completion_tokens":4000,"total_tokens":4100}
            }
            """);

        var result = client.GetResponse(CreatePage(), "listing input");

        Assert.True(result.WaitingAIRequest);
        Assert.Null(result.ResponseText);
        Assert.Contains("truncated by max_tokens", result.Diagnostic);
    }

    private static LocalAIListingParserClient CreateClient(string responseBody)
    {
        HttpClient httpClient = new(new StubHandler(responseBody));
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
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
    }

    private sealed class NullLogger : IApplicationLogger
    {
        public void WriteError(string source, string message) { }

        public void WriteInfo(string source, string message) { }
    }
}
