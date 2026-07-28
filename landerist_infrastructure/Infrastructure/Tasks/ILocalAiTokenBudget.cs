namespace landerist_library.Infrastructure.Tasks;

public interface ILocalAiTokenBudget
{
    int Calculate(LocalAiParsingTaskOptions options);
}
