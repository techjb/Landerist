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
        public const string LogKeyScrapper = "scrapper";

        private static void WriteLog(string logKey, string source, string text)
        {
            if (text.Equals(string.Empty))
            {
                return;
            }
            if (Config.LOGS_ENABLED)
            {
                WriteLogDB(logKey, source, text);
            }
        }

        private static bool WriteLogDB(string logKey, string source, string text)
        {
            string query =
                "INSERT INTO " + TABLE_LOGS + " " +
                "VALUES(GETDATE(), @LogKey, @Source, @Text)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                    { "LogKey", logKey },
                    { "Source", source },
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

        private static string GetText(string text, Exception exception)
        {
            return text + " " + GetText(exception);
        }

        private static string GetText(Exception exception)
        {
            string stackTrace = GetStackTrace(exception);

            return
                "Message: " + exception.Message + "\r\n" +
                "Source: " + exception.Source + "\r\n" +
                "StackTrace: " + stackTrace + "\r\n" +
                "TargetSite: " + exception.TargetSite + "\r\n";
        }

        private static string GetStackTrace(Exception exception)
        {
            string stackTrace = exception.StackTrace ?? "";

            System.Diagnostics.StackTrace trace = new(exception, true);
            if (trace.FrameCount > 0)
            {
                System.Diagnostics.StackFrame? frame = trace.GetFrame(0);
                if (frame != null)
                {
                    int lineNumber = frame.GetFileLineNumber();
                    stackTrace += " \r\nLine number: " + lineNumber;
                }
            }
            return stackTrace;
        }
             

        #region Write Logs

        public static void WriteLogErrors(string source, string text)
        {
            WriteLog(LogKeyError, source, text);
        }

        public static void WriteLogErrors(string source, string text, Exception exception)
        {
            string textError = GetText(text, exception);
            WriteLogErrors(source, textError);
        }

        public static void WriteLogErrors(string source, Exception exception)
        {
            string textError = GetText(exception);
            WriteLogErrors(source, textError);
        }

        public static void WriteLogErrors(string source, Uri uri, Exception exception)
        {
            source += " " + uri.ToString();
            WriteLogErrors(source, exception);
        }

        public static void WriteLogErrors(Uri uri, Exception exception)
        {
            string source = uri.ToString();
            WriteLogErrors(source, exception);
        }

        public static void WriteLogInfo(string source, string text)
        {
            WriteLog(LogKeyInfo, source, text);
        }

        public static void WriteLogInfo(Exception exception)
        {
            WriteLogInfo(string.Empty, exception);
        }

        public static void WriteLogInfo(string source, Exception exception)
        {
            string textError = GetText(exception);
            WriteLogInfo(source, textError);
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
