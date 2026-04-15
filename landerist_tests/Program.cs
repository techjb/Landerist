using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Scrape;
using System.Runtime.InteropServices;


namespace landerist_tests
{
    partial class Program
    {
        private static DateTime DateStart;
        private static int IsEnding;
        private static readonly ManualResetEventSlim ExitSignal = new(false);

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ConsoleEventDelegate Handler = new(ConsoleEventHandler);
        public delegate void KeyPressedHandler(ConsoleKeyInfo key);
        public static event KeyPressedHandler? OnKeyPressed;
        private static readonly Scraper Scrapper = new();

        static void Main()
        {
            Console.Title = "Landerist Tests";
            Start();
            Run();
            //ExitSignal.Wait();
            End();
        }

        private static void Start()
        {
            //RegisterExitEvents();
            //SetCtrlDListener();

            DateStart = DateTime.Now;            
            Log.DeleteCurentMachineLogs();
            Log.Console("Started. Machine: " + Config.MACHINE_NAME + " Version: " + Config.VERSION);
        }

        private static void RegisterExitEvents()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetConsoleCtrlHandler(Handler, true);
            }

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                ExitSignal.Set();
                End();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) => End();
        }

        static void SetCtrlDListener()
        {
            OnKeyPressed += keyInfo =>
            {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.D)
                {
                    Console.WriteLine("¡Ctrl + D detectado!");
                    ExitSignal.Set();
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
                {
                    ExitSignal.Set();
                    return;
                }
            }
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            Console.WriteLine(eventType);
            if (eventType is 0 or 2 or 5 or 6)
            {
                ExitSignal.Set();
                End();
            }
            return false;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static void End()
        {
            if (Interlocked.Exchange(ref IsEnding, 1) == 1)
            {
                return;
            }

            //ServiceTasks.Stop();
            Scrapper.Stop();
            var duration = (DateTime.Now - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.Console("Stopped. Version: " + Config.VERSION + " Duration: " + duration);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Beep(500, 500);
            }
        }

        private static void Run()
        {
            Config.SetOnlyDatabaseToProduction();

            UrlsTests.Run();
            WebsitesTests.Run();
            PagesTests.Run();
            ScrapperTests.Run();
            DownloadersTests.Run();
            ListingParserTests.Run();
            LocalAITests.Run();
            LocationParserTests.Run();
            IndexTests.Run();
            BackupTests.Run();
            StatisticsTests.Run();
            ListingsTests.Run();
            InsertTests.Run();
            LanderistComTests.Run();
            DataBaseTests.Run();
            TasksTests.Run();
            LocalIsListingTests.Run();
        }
    }
}

