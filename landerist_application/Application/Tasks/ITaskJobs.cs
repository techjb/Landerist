namespace landerist_library.Application.Tasks;

public interface IScrapeTaskJob
{
    void Prepare();

    void Run();

    Task RunAsync(CancellationToken cancellationToken = default);

    void Stop();

    Task StopAsync(CancellationToken cancellationToken = default);
}

public interface ILocalAiTaskJob
{
    void Run();

    void Stop();
}

public interface IRecurringTaskJob
{
    void Run();
}
