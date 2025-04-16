using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Tasks;
using System.Runtime.InteropServices;

namespace landerist_console
{
    partial class Program
    {
        private static DateTime DateStart;
        private static readonly ServiceTasks ServiceTasks = new();

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ConsoleEventDelegate Handler = new(ConsoleEventHandler);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, [MarshalAs(UnmanagedType.Bool)] bool add);

        static void Main()
        {
            Console.Title = "Landerist Console";
            Start();
            Run();            
            End();
        }

        private static void Start()
        {
            SetConsoleCtrlHandler(Handler, true);
            DateStart = DateTime.Now;
            Config.SetToProduction();            
            Log.Delete();
            Log.WriteInfo("landerist_console", "Started. Version: " + Config.VERSION);
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            if (eventType == 2)
            {
                End();
            }
            return false;
        }

        private static void End()
        {
            ServiceTasks.Stop();

            var duration = (DateTime.Now - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.WriteInfo("landerist_console", "Stopped. Version: " + Config.VERSION + " Duration: " + duration);
        }

        private static void Run()
        {
            ServiceTasks.Start();

            Console.WriteLine("ReadLine or Close to exit..");
            Console.ReadLine();
        }
    }
}