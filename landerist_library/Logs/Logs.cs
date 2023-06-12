using landerist_library.Configuration;
using landerist_library.Database;
using System.Data;

namespace landerist_library.Logs
{
    public class Log
    {
        private const string TABLE_LOGS = "[LOGS]";

        public const string LogKeyError = "error";
        public const string LogKeyInfo = "info";

        private static void WriteLog(string logKey, string text)
        {
            if (text.Equals(string.Empty))
            {
                return;
            }            
            if (Config.LOGS_ENABLED)
            {
                WriteLogDB(logKey, text);
            }
            else
            {
                Console.WriteLine(logKey.ToString() + ": " + text);
            }
        }

        private static void WriteLogDB(string logKey, string text)
        {
            string query =
                "INSERT INTO " + TABLE_LOGS + " " +
                "VALUES(GETDATE(), @LogKey, @Text)";

            new DataBase().Query(query, new Dictionary<string, object?> {
                    { "LogKey", logKey },
                    { "Text", text.Trim() }
                });
        }

        public static DataTable ReadLog(string logKey, int top = 200)
        {
            string query =
                "SELECT TOP " + top + " * " +
                "FROM " + TABLE_LOGS + " " +
                "WHERE LogKey = @LogKey " +
                "ORDER BY [Date] DESC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                { "LogKey", logKey }
            });
        }

        public static List<string> ReadLogKeys()
        {
            string query =
                "SELECT DISTINCT LogKey " +
                "FROM " + TABLE_LOGS;
            return new DataBase().QueryListString(query);
        }

        public static void DeleteLog(string logKey)
        {
            string query =
                "DELETE FROM " + TABLE_LOGS + " " +
                "WHERE LogKey = @LogKey";

            new DataBase().Query(query, new Dictionary<string, object?> {
                    { "LogKey", logKey }
                });
        }


        private static string GetText(Exception exception)
        {
            return
                "Message: " + exception.Message + Environment.NewLine +
                "Source: " + exception.Source + Environment.NewLine +
                "StackTrace: " + exception.StackTrace + Environment.NewLine +
                "TargetSite: " + exception.TargetSite;
        }

        #region Write Logs

        public static void WriteLogErrors(string text)
        {
            WriteLog(LogKeyError, text);
        }

        public static void WriteLogErrors(Exception exception)
        {
            WriteLogErrors(string.Empty, exception);
        }

        public static void WriteLogErrors(string text, Exception exception)
        {
            string textError = GetText(exception);
            if (!text.Equals(string.Empty))
            {
                textError = text + Environment.NewLine + textError;
            }
            WriteLogErrors(textError);
        }

        public static void WriteLogErrors(Uri uri, Exception exception)
        {
            WriteLogErrors("Uri: " + uri.ToString(), exception);
        }

        public static void WriteLogInfo(string text)
        {
            WriteLog(LogKeyInfo, text);
        }

        public static void WriteLogInfo(Exception exception)
        {
            WriteLogInfo(string.Empty, exception);
        }

        public static void WriteLogInfo(string text, Exception exception)
        {
            WriteLogInfo((text.Equals(string.Empty) ? string.Empty : text + " ") + GetText(exception));
        }

        #endregion Write Logs

        public static bool CleanTable()
        {
            string query =
                "DELETE FROM " + TABLE_LOGS + " " +
                "WHERE [Date] < DATEADD(YEAR, -1, GETDATE())";

            return new DataBase().Query(query);
        }
    }
}
