namespace landerist_library.Application.Logging;

public interface IApplicationLogger
{
    void WriteError(string source, string message);

    void WriteInfo(string source, string message);
}
