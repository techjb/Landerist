using landerist_library.Application.Logging;
using landerist_library.Logs;

namespace landerist_library.Infrastructure.Logging;

public sealed class LegacyApplicationLogger : IApplicationLogger
{
    public void WriteError(string source, string message) =>
        Log.WriteError(source, message);

    public void WriteInfo(string source, string message) =>
        Log.WriteInfo(source, message);
}
