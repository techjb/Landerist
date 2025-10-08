using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Tasks;

namespace landerist_console
{
    partial class Program
    {
        private static DateTime? DateStart = null;
        private static readonly TasksService ServiceTasks = new();

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ManualResetEvent ManualResetEvent = new(false);
        public delegate void KeyPressedHandler(ConsoleKeyInfo key);
        public static event KeyPressedHandler? OnKeyPressed;

        static void Main()
        {
            Console.Title = "Landerist Console " + Config.VERSION;
            Start();
            Run();
        }

        private static void Start()
        {
            if (!Config.IsLocalAIMachine())
            {
                Console.WriteLine("Ctrl+D to daily tasks.");
                DateStart = DateTime.Now;
                SetCtrlDListener();
            }
            Console.CancelKeyPress += (s, e) =>
            {
                ManualResetEvent.Set();
                End();
            };
            Console.WriteLine("Press Ctrl+C to exit.");

            //DateStart = DateTime.Now;           

            Console.WriteLine("Setting to production");
            Config.SetToProduction();
            Console.WriteLine("Deleting logs..");
            Log.DeleteCurentMachineLogs();
            Log.WriteInfo("landerist_console", "Started. Machine: " + Config.MACHINE_NAME + " Version: " + Config.VERSION);
        }

        static void SetCtrlDListener()
        {
            if (Config.IsLocalAIMachine())
            {
                return;
            }

            OnKeyPressed += keyInfo =>
            {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.D)
                {
                    ServiceTasks.PerformDailyTask();
                }
            };
            Thread inputThread = new(KeyboardListener);
            inputThread.Start();
        }
        static void KeyboardListener()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                OnKeyPressed?.Invoke(keyInfo);

                if (keyInfo.Key == ConsoleKey.Escape)
                    Environment.Exit(0);
            }
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            if (eventType == 2)
            {
                End();
            }
            return false;
        }

        private static void Run()
        {
            ServiceTasks.Start();
            ManualResetEvent.WaitOne();
        }

        private static void End()
        {
            Log.WriteInfo("landerist_console", "Stopping...");
            ServiceTasks.Stop();

            if (DateStart is null)
            {
                return;
            }
            var duration = (DateTime.Now - (DateTime)DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.WriteInfo("landerist_console", "Stopped. Version: " + Config.VERSION + " Duration: " + duration);
        }
    }
}