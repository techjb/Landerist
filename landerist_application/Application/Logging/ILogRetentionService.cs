namespace landerist_library.Application.Logging;

public interface ILogRetentionService
{
    LogRetentionResult Clean();
}

public sealed record LogRetentionResult(int InformationDeleted, int ErrorsDeleted)
{
    public int TotalDeleted => checked(InformationDeleted + ErrorsDeleted);
}
