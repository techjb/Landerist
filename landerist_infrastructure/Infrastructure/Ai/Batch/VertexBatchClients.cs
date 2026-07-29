using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Google.Cloud.Storage.V1;
using landerist_library.Application.Logging;

namespace landerist_library.Infrastructure.Ai.Batch;

public sealed record VertexBatchOptions(
    string CredentialJson,
    string ProjectId,
    string Location,
    string Model,
    string BucketName,
    string LocalDirectory)
{
    public VertexBatchOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(CredentialJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(ProjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Location);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        ArgumentException.ThrowIfNullOrWhiteSpace(BucketName);
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalDirectory);
        return this;
    }
}

public enum VertexBatchJobState
{
    Pending,
    Succeeded,
    Failed,
}

public sealed record VertexBatchJobResult(
    VertexBatchJobState State,
    string? InputObject,
    string? OutputDirectory);

public sealed class VertexBatchJobClient(
    VertexBatchOptions options,
    IApplicationLogger logger)
{
    private readonly VertexBatchOptions _options = options.Validate();

    public string? Create(string inputObject)
    {
        string outputObject = inputObject
            .Replace("input/", "output/", StringComparison.Ordinal)
            .Replace("_input", "_output", StringComparison.Ordinal);
        try
        {
            BatchPredictionJob job = Client()
                .CreateBatchPredictionJob(
                    new CreateBatchPredictionJobRequest
                    {
                        Parent = Parent,
                        BatchPredictionJob = new BatchPredictionJob
                        {
                            Name = inputObject,
                            DisplayName = inputObject,
                            Model =
                                "publishers/google/models/"
                                + _options.Model,
                            InputConfig =
                                new BatchPredictionJob.Types.InputConfig
                                {
                                    InstancesFormat = "jsonl",
                                    GcsSource = new GcsSource
                                    {
                                        Uris =
                                        {
                                            GcsUri(inputObject),
                                        },
                                    },
                                },
                            OutputConfig =
                                new BatchPredictionJob.Types.OutputConfig
                                {
                                    PredictionsFormat = "jsonl",
                                    GcsDestination = new GcsDestination
                                    {
                                        OutputUriPrefix =
                                            GcsUri(outputObject),
                                    },
                                },
                        },
                    });
            return job.Name;
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexBatchJobClient) + ".Create",
                exception.ToString());
            return null;
        }
    }

    public VertexBatchJobResult? Get(string name)
    {
        try
        {
            BatchPredictionJob job = Client().GetBatchPredictionJob(
                new GetBatchPredictionJobRequest { Name = name });
            return new VertexBatchJobResult(
                job.State switch
                {
                    JobState.Succeeded =>
                        VertexBatchJobState.Succeeded,
                    JobState.Failed =>
                        VertexBatchJobState.Failed,
                    _ => VertexBatchJobState.Pending,
                },
                StripBucket(job.InputConfig?.GcsSource?.Uris.FirstOrDefault()),
                StripBucket(job.OutputInfo?.GcsOutputDirectory));
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexBatchJobClient) + ".Get",
                exception.ToString());
            return null;
        }
    }

    public void DeleteCompletedBefore(DateTime cutoff)
    {
        JobServiceClient client = Client();
        foreach (BatchPredictionJob job in
            client.ListBatchPredictionJobs(
                new ListBatchPredictionJobsRequest { Parent = Parent }))
        {
            if (job.State != JobState.Succeeded
                || job.EndTime.ToDateTime() >= cutoff)
            {
                continue;
            }

            try
            {
                client.DeleteBatchPredictionJob(
                        new DeleteBatchPredictionJobRequest
                        {
                            Name = job.Name,
                        })
                    .PollUntilCompleted();
            }
            catch (Exception exception)
            {
                logger.WriteError(
                    nameof(VertexBatchJobClient)
                        + ".DeleteCompletedBefore",
                    exception.ToString());
            }
        }
    }

    private string Parent =>
        $"projects/{_options.ProjectId}/locations/{_options.Location}";

    private string GcsUri(string value) =>
        $"gs://{_options.BucketName}/{value}";

    private string? StripBucket(string? value)
    {
        string prefix = $"gs://{_options.BucketName}/";
        return value?.StartsWith(prefix, StringComparison.Ordinal) == true
            ? value[prefix.Length..]
            : value;
    }

    private JobServiceClient Client() =>
        new JobServiceClientBuilder
        {
            Endpoint =
                $"{_options.Location}-aiplatform.googleapis.com",
            GoogleCredential = Credential,
        }.Build();

    private GoogleCredential Credential =>
        CredentialFactory
            .FromJson<ServiceAccountCredential>(
                _options.CredentialJson)
            .ToGoogleCredential();
}

public sealed class VertexCloudStorageClient(
    VertexBatchOptions options,
    IApplicationLogger logger)
{
    private readonly VertexBatchOptions _options = options.Validate();

    public string? Upload(string filePath)
    {
        string objectName = "input/" + Path.GetFileName(filePath);
        try
        {
            using FileStream stream = File.OpenRead(filePath);
            return Client().UploadObject(
                    _options.BucketName,
                    objectName,
                    "application/jsonl",
                    stream)
                .Name;
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexCloudStorageClient) + ".Upload",
                exception.ToString());
            return null;
        }
    }

    public string? Download(string objectName)
    {
        string fileName = Path.GetFileName(objectName);
        string localPath = Path.Combine(
            _options.LocalDirectory,
            fileName);
        try
        {
            Directory.CreateDirectory(_options.LocalDirectory);
            using FileStream stream = File.Create(localPath);
            Client().DownloadObject(
                _options.BucketName,
                objectName,
                stream);
            return localPath;
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(VertexCloudStorageClient) + ".Download",
                exception.ToString());
            return null;
        }
    }

    public void DeleteBefore(DateTime cutoff)
    {
        StorageClient client = Client();
        foreach (Google.Apis.Storage.v1.Data.Object item in
            client.ListObjects(_options.BucketName))
        {
            if (item.TimeCreatedDateTimeOffset >= cutoff)
            {
                continue;
            }

            try
            {
                client.DeleteObject(_options.BucketName, item.Name);
            }
            catch (Exception exception)
            {
                logger.WriteError(
                    nameof(VertexCloudStorageClient) + ".DeleteBefore",
                    exception.ToString());
            }
        }
    }

    private StorageClient Client() =>
        StorageClient.Create(
            CredentialFactory
                .FromJson<ServiceAccountCredential>(
                    _options.CredentialJson)
                .ToGoogleCredential());
}
