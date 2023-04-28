using System.Data;
using landerist_library.Configuration;
using landerist_library.Database;

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
            if (Environment.UserInteractive && 
                !Config.IsConfigurationProduction())
            {
                Console.WriteLine(logKey.ToString() + ": " + text);
            }

            WriteLogDB(logKey, text);
            //else
            //{
            //    WriteLogDB(logKey, text);
            //}
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


        private static string GetText(Exception exception, string additionalInformation = "")
        {
            if (additionalInformation != "")
            {
                additionalInformation = "<br />" +
                    "Aditional information: " + additionalInformation.Trim();
            }
            return
                "Message: " + exception.Message + "<br />" +
                "Source: " + exception.Source + "<br />" +
                "StackTrace: " + exception.StackTrace + "<br />" +
                "StackTrace: " + exception.TargetSite +
                additionalInformation;
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
            WriteLogErrors((text.Equals(string.Empty) ? string.Empty : text + " ") + GetText(exception));
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
