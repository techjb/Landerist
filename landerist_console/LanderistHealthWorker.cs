using System.Text.Json;
using landerist_library.Application.Logging;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using Microsoft.Extensions.Hosting;

namespace landerist_console;

internal sealed record HealthPublisherOptions
{
    public HealthPublisherOptions(string filePath, TimeSpan interval)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        FilePath = filePath;
        Interval = interval;
    }

    public string FilePath { get; }
    public TimeSpan Interval { get; }
}

internal sealed class LanderistHealthWorker(
    IDatabaseFactory databaseFactory,
    ITaskHealthRegistry taskHealth,
    HealthPublisherOptions options,
    IApplicationLogger logger,
    TimeProvider timeProvider) : BackgroundService
{
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
    }
}
