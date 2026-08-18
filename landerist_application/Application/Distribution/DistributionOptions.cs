namespace landerist_library.Application.Distribution;

public sealed record DistributionOptions(
    string ExportDirectory,
    string TemplatesDirectory,
    string OutputDirectory,
    string DownloadsBucket,
    string AwsAccessKeyId,
    string AwsSecretAccessKey,
    string CloudFrontDistributionId)
{
    public static DistributionOptions Empty { get; } = new(
        ".", ".", ".", "unconfigured", string.Empty, string.Empty, string.Empty);

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ExportDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(TemplatesDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(OutputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(DownloadsBucket);
    }
}
