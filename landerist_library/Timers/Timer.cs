﻿using landerist_library.Configuration;
using landerist_library.Database;
using OpenCvSharp;
using System.Data;

namespace landerist_library.Timers
{
    public class Timer
    {
        private const string TABLE_TIMERS = "[TIMERS]";

        public const string TimerKeyChatGPT = "chatgpt";
        public const string TimerKeyDownloadPage = "downloadpage";

        private static void SaveTimer(string timerKey, string source, DateTime dateStart)
        {
            if (!Config.TIMERS_ENABLED)
            {
                return;
            }
            double milliseconds = (DateTime.Now - dateStart).TotalMilliseconds;
            SaveTimer(timerKey, source, milliseconds);
        }

        private static bool SaveTimer(string timerKey, string source, double milliseconds)
        {
            string query =
                "INSERT INTO " + TABLE_TIMERS + " " +
                "VALUES(GETDATE(), @TimerKey, @Source, @Milliseconds)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                    { "TimerKey", timerKey },
                    { "Source", source },
                    { "Milliseconds", milliseconds }
                });           
        }

        public static void DeleteLog(string timerKey)
        {
            string query =
                "DELETE FROM " + TABLE_TIMERS + " " +
                "WHERE TimerKey = @TimerKey";

            new DataBase().Query(query, new Dictionary<string, object?> {
                    { "TimerKey", timerKey }
                });
        }


        #region Save timer

        public static void ChatGPT(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyChatGPT, source, dateStart);
        }

        public static void DownloadPage(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyDownloadPage, source, dateStart);
        }
       

        #endregion Write Logs
    }
}