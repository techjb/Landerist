using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Ai.LocalAI;

namespace landerist_integration_tests;

public sealed class LocalAIAddressSelectionTests
{
    [LocalAiTheory]
    [InlineData("Calle Mayor 12, Madrid", "Calle Mayor 12, Madrid")]
    [InlineData("C/ Mayor 12, 2º A, Madrid", "Calle Mayor 12, Madrid")]
    [InlineData("  CALLE MAYOR 12, MADRID  ", "Calle Mayor 12, Madrid")]
    [InlineData("Calle Mayor 14, Madrid", null)]
    [InlineData("Calle Mayor 12, Toledo", null)]
    [InlineData("Calle Menor 12, Madrid", null)]
    public void Select_RealQwenMatchesOnlyEquivalentAddress(string search, string? expected)
    {
        LocalAIAddressCandidateSelector selector = new(
            new LocalAIListingParserOptions(
                Environment.GetEnvironmentVariable("LANDERIST_TEST_LOCALAI_HOST")!),
            new Logger());

        Assert.Equal(expected, selector.Select(search,
            ["Calle Mayor 10, Madrid", "Calle Mayor 12, Madrid", "Calle Mayor 12, Sevilla"]));
    }

    private sealed class Logger : IApplicationLogger
    {
        public void WriteError(string source, string message) =>
            throw new InvalidOperationException(source + ": " + message);
        public void WriteInfo(string source, string message) { }
    }
}

public sealed class LocalAiTheoryAttribute : TheoryAttribute
{
    public LocalAiTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LANDERIST_TEST_LOCALAI_HOST")))
            Skip = "Set LANDERIST_TEST_LOCALAI_HOST to run real Qwen address queries.";
    }
}
