using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Logs;
using Microsoft.Extensions.Hosting;

namespace landerist_console;

internal sealed class LanderistWorker : IHostedService
{
    private readonly TasksService _tasks;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly LanderistRuntimeOptions _runtimeOptions;
    private DateTime? _startedAt;

    public LanderistWorker(
        TasksService tasks,
        IHostApplicationLifetime applicationLifetime,
        LanderistRuntimeOptions runtimeOptions)
    {
        _tasks = tasks;
        _applicationLifetime = applicationLifetime;
        _runtimeOptions = runtimeOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runtimeOptions.Role == LanderistExecutionRole.Principal)
        {
            Console.WriteLine("Ctrl+D to run daily tasks.");
            _startedAt = DateTime.Now;
            StartKeyboardListener();
        }

        Console.WriteLine("Press Ctrl+C to exit.");
        Log.WriteInfo(
            "landerist_console",
            "Started. Machine: " + _runtimeOptions.Execution.MachineName +
            " Version: " + _runtimeOptions.Execution.Version);

        _tasks.Start();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.WriteInfo(
            "landerist_console",
            "Stopping Version: " + _runtimeOptions.Execution.Version + " ..");
        await _tasks.StopAsync(cancellationToken).ConfigureAwait(false);

        if (_startedAt is not null)
        {
            string duration = (DateTime.Now - _startedAt.Value)
                .ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.WriteInfo(
                "landerist_console",
                "Stopped. Version: " + _runtimeOptions.Execution.Version +
                " Duration: " + duration);
        }
    }

    private void StartKeyboardListener()
    {
        if (_runtimeOptions.Role == LanderistExecutionRole.LocalAi || Console.IsInputRedirected)
        {
            return;
        }

        Thread inputThread = new(KeyboardListener)
        {
            IsBackground = true,
            Name = "Landerist console keyboard listener"
        };
        inputThread.Start();
    }

    private void KeyboardListener()
    {
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                keyInfo.Key == ConsoleKey.D)
            {
                _tasks.PerformDailyTask(null);
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                _applicationLifetime.StopApplication();
                return;
            }
        }
    }
}
