using System.Net;
using System.Text;
using System.Text.Json;
using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Ai.LocalAI;

namespace landerist_unit_tests;

public sealed class LocalAIAddressCandidateSelectorTests
{
    [Fact]
    public void Select_ReturnsOriginalCandidateAndSendsLocalModelWithSchema()
    {
        Handler handler = new("{\"indice\":1}");
        LocalAIAddressCandidateSelector selector = Create(handler);
        string[] candidates = ["Calle Mayor 10, Madrid", "Calle Mayor 12, Madrid"];

        Assert.Equal(candidates[1], selector.Select("C/ Mayor 12, 2º A, Madrid", candidates));
        Assert.Equal("http://qwen.test:8000/v1/chat/completions", handler.Url);
        using JsonDocument request = JsonDocument.Parse(handler.Body!);
        Assert.Equal(LocalAIListingParserOptions.DefaultModel,
            request.RootElement.GetProperty("model").GetString());
        Assert.Equal("json_schema", request.RootElement.GetProperty("response_format")
            .GetProperty("type").GetString());
        Assert.False(request.RootElement.GetProperty("chat_template_kwargs")
            .GetProperty("enable_thinking").GetBoolean());
        using JsonDocument input = JsonDocument.Parse(request.RootElement
            .GetProperty("messages")[1].GetProperty("content").GetString()!);
        Assert.Equal(candidates[1], input.RootElement.GetProperty("candidatas")[1].GetString());
    }

    [Theory]
    [InlineData("{\"indice\":null}")]
    [InlineData("{\"indice\":-1}")]
    [InlineData("{\"indice\":2}")]
    [InlineData("{\"indice\":\"0\"}")]
    [InlineData("{\"indice\":0.5}")]
    [InlineData("{}")]
    [InlineData("not json")]
    public void Select_RejectsMissingOrInvalidSelection(string content)
    {
        Assert.Null(Create(new Handler(content)).Select("Mayor 12", ["Mayor 10", "Mayor 14"]));
    }

    [Fact]
    public void Select_RejectsTruncatedResponse()
    {
        Assert.Null(Create(new Handler("{\"indice\":0}", Finish: "length"))
            .Select("Mayor 12", ["Mayor 12"]));
    }

    [Fact]
    public void Select_HandlesServerError()
    {
        Assert.Null(Create(new Handler("{}", Status: HttpStatusCode.ServiceUnavailable))
            .Select("Mayor 12", ["Mayor 12"]));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Select_SkipsEmptySearch(string search)
    {
        Handler handler = new("{}");
        Assert.Null(Create(handler).Select(search, ["Mayor 12"]));
        Assert.Null(handler.Body);
    }

    [Fact]
    public void Select_SkipsEmptyCandidates()
    {
        Handler handler = new("{}");
        Assert.Null(Create(handler).Select("Mayor 12", []));
        Assert.Null(handler.Body);
    }

    private static LocalAIAddressCandidateSelector Create(Handler handler) => new(
        new LocalAIListingParserOptions("qwen.test"), new Logger(), new HttpClient(handler));

    private sealed class Handler(string Content, string Finish = "stop",
        HttpStatusCode Status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public string? Body { get; private set; }
        public string? Url { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Url = request.RequestUri!.ToString();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(Status)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    choices = new[] { new { finish_reason = Finish,
                        message = new { content = Content } } }
                }), Encoding.UTF8, "application/json")
            };
        }
    }
    private sealed class Logger : IApplicationLogger
    {
        public void WriteError(string source, string message) { }
        public void WriteInfo(string source, string message) { }
    }
}
