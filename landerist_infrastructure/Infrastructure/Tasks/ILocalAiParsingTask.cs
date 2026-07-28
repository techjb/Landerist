namespace landerist_library.Infrastructure.Tasks;

public interface ILocalAiParsingTask
{
    void ProcessPages(CancellationToken cancellationToken = default);

    void Stop();
}
