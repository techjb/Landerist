using landerist_library.Configuration;
using System.Diagnostics;

namespace landerist_library.Downloaders.Puppeteer
{
    public static class CromeKiller
    {
        public static void KillChrome()
        {
            if (!Config.IsConfigurationProduction())
            {
                return;
            }

            KillCromeProcess();
            KillCromeByTaskKill();
        }

        public static void KillCromeProcess()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("chrome");
                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception exception)
                    {
                        Logs.Log.WriteError("CromeKiller", exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("CromeKiller KillCromeProcess", exception);
            }
        }

        public static void KillCromeByTaskKill()
        {
            try
            {
                using Process process = new();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM \"chrome.exe\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                process.Start();
                process.WaitForExit();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("CromeKiller KillCromeByTaskKill", exception);
            }
        }
    }
}
