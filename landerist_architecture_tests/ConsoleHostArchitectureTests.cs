namespace landerist_architecture_tests;

public sealed class ConsoleHostArchitectureTests
{
    [Fact]
    public void Program_UsesGenericHostForProcessLifecycle()
    {
        string program = ReadConsoleSource("Program.cs");

        Assert.Contains("Host.CreateApplicationBuilder(args)", program);
        Assert.Contains("AddHostedService<LanderistWorker>()", program);
        Assert.DoesNotContain("ManualResetEvent", program);
        Assert.DoesNotContain("Console.CancelKeyPress", program);
        Assert.DoesNotContain("Environment.Exit", program);
    }

    [Fact]
    public void Program_DelegatesObjectGraphConstructionToCompositionRoot()
    {
        string program = ReadConsoleSource("Program.cs");
        string composition = ReadConsoleSource("LanderistServiceComposition.cs");

        Assert.Contains(
            "LanderistServiceComposition.CreateTasksService()",
            program);
        Assert.Contains("CreateTasksService()", composition);
        Assert.True(
            File.ReadLines(GetConsolePath("Program.cs")).Count() <= 30,
            "Program must remain a small host bootstrapper.");
    }

    private static string ReadConsoleSource(string fileName) =>
        File.ReadAllText(GetConsolePath(fileName));

    private static string GetConsolePath(string fileName) =>
        Path.Combine(FindRepositoryRoot(), "landerist_console", fileName);

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Landerist.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing Landerist.sln.");
    }
}