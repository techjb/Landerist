namespace landerist_library.Application.Tasks;

public interface IScrapeTaskJob
{
    void Prepare();

    void Run();

    void Stop();
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
