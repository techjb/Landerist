using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Timers
{
    public static class Timer
    {
        private const string TABLE_TIMERS = "[TIMERS]";

        public const string TimerKeyVertexAI = "vertexai";
        public const string TimerKeyChatGPT = "chatgpt";
        public const string TimerKeyOpenAI = TimerKeyChatGPT;
        public const string TimerKeyLocalAI = "localai";
        public const string TimerKeyDownloadPage = "downloadpage";

        public static void SaveTimer(string timerKey, string source, DateTime dateStart)
        {
            if (!Config.TIMERS_ENABLED)
            {
                return;
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(timerKey);
            source ??= string.Empty;

            double milliseconds = (DateTime.Now - dateStart).TotalMilliseconds;

            if (milliseconds < 0)
            {
                milliseconds = 0;
            }

            SaveTimer(timerKey, source, milliseconds);
        }

        private static bool SaveTimer(string timerKey, string source, double milliseconds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(timerKey);
            source ??= string.Empty;

            string query =
                "INSERT INTO " + TABLE_TIMERS + " " +
                "VALUES(GETDATE(), @TimerKey, @Source, @Milliseconds)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                { "TimerKey", timerKey },
                { "Source", source },
                { "Milliseconds", milliseconds }
            });
        }

        public static void Delete(string timerKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(timerKey);

            string query =
                "DELETE FROM " + TABLE_TIMERS + " " +
                "WHERE TimerKey = @TimerKey";

            new DataBase().Query(query, new Dictionary<string, object?> {
                { "TimerKey", timerKey }
            });
        }

        #region Save timer

        public static void SaveTimerOpenAI(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyOpenAI, source, dateStart);
        }

        public static void SaveTimerChatGPT(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyChatGPT, source, dateStart);
        }

        public static void SaveTimerLocalAI(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyLocalAI, source, dateStart);
        }

        public static void SaveTimerVertexAI(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyVertexAI, source, dateStart);
        }

        public static void SaveTimerDownloadPage(string source, DateTime dateStart)
        {
            SaveTimer(TimerKeyDownloadPage, source, dateStart);
        }

        #endregion
    }
}
