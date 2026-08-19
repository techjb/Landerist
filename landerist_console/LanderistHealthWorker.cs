using System.Text.Json;
using landerist_library.Application.Logging;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Infrastructure.Logging;
using Microsoft.Extensions.Hosting;

namespace landerist_console;

internal sealed record HealthPublisherOptions
{
    public HealthPublisherOptions(
        string filePath,
        TimeSpan interval,
        Uri? healthchecksPingUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        FilePath = filePath;
        Interval = interval;
        HealthchecksPingUri = healthchecksPingUri;
    }

    public string FilePath { get; }
    public TimeSpan Interval { get; }
    public Uri? HealthchecksPingUri { get; }
}

internal sealed class LanderistHealthWorker(
    IDatabaseFactory databaseFactory,
    ITaskHealthRegistry taskHealth,
    HealthPublisherOptions options,
    IApplicationLogger logger,
    TimeProvider timeProvider) : BackgroundService
{
    private readonly HttpClient _heartbeatClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PublishAsync(stoppingToken).ConfigureAwait(false);
        using PeriodicTimer timer = new(options.Interval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            await PublishAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task PublishAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetLocalNow();
        bool sqlAvailable;
        string? sqlError = null;
        bool snapshotPublished = false;
        try
        {
            sqlAvailable = await databaseFactory.Create().QueryBoolAsync(
                "SELECT CAST(1 AS bit)",
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            sqlAvailable = false;
            sqlError = exception.Message;
        }

        IReadOnlyList<TaskHealthSnapshot> tasks = taskHealth.Snapshot(now);
        string status = !sqlAvailable
            ? "unavailable"
            : tasks.Any(task => task.Status == "degraded")
                ? "degraded"
                : "healthy";
        var document = new
        {
            status,
            observedAt = now,
            liveness = true,
            readiness = new { sql = sqlAvailable, error = sqlError },
            tasks
        };

        try
        {
            string path = Path.GetFullPath(options.FilePath);
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = path + ".tmp";
            await File.WriteAllTextAsync(
                temporaryPath,
                JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, path, overwrite: true);
            snapshotPublished = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            try
            {
                logger.WriteError(nameof(LanderistHealthWorker), exception.ToString());
            }
            catch
            {
                // Health publication must remain alive even if logging is unavailable.
            }
        }

        await SendHeartbeatAsync(
            status == "healthy" && snapshotPublished,
            cancellationToken).ConfigureAwait(false);
    }

    public override void Dispose()
    {
        _heartbeatClient.Dispose();
        base.Dispose();
    }

    private async Task SendHeartbeatAsync(
        bool healthy,
        CancellationToken cancellationToken)
    {
        if (options.HealthchecksPingUri is null)
        {
            return;
        }

        try
        {
            Uri uri = HealthchecksUriBuilder.GetHeartbeatUri(
                options.HealthchecksPingUri,
                healthy);
            using HttpResponseMessage response = await _heartbeatClient
                .GetAsync(uri, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            try
            {
                logger.WriteError(
                    nameof(LanderistHealthWorker),
                    $"Healthchecks heartbeat failed: {exception}");
            }
            catch
            {
                // A failed alert transport must not terminate health publication.
            }
        }
    }
}
