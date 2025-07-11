﻿using landerist_library.Configuration;
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
        public const string LogKeyBatch = "batch";

        private static void Write(string logKey, string source, string text)
        {
            if (text.Equals(string.Empty))
            {
                return;
            }
            if (Config.LOGS_ENABLED)
            {
                WriteDB(logKey, source, text);
            }
        }

        public static void Console(string text)
        {
            DateTime date = DateTime.Now;
            System.Console.WriteLine($"{date:HH\\:mm\\:ss} {text}");
        }

        private static bool WriteDB(string logKey, string source, string text)
        {
            string query =
                "INSERT INTO " + TABLE_LOGS + " " +
                "VALUES(@Date, @MachineName, @LogKey, @Source, @Text)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                { "Date", DateTime.Now},
                { "MachineName", Config.MACHINE_NAME},
                { "LogKey", logKey },
                { "Source", source },
                { "Text", text.Trim() }
            });
        }

        public static DataTable Read(string logKey, int top = 200)
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

        public static List<string> ReadKeys()
        {
            string query =
                "SELECT DISTINCT LogKey " +
                "FROM " + TABLE_LOGS;
            return new DataBase().QueryListString(query);
        }

        public static void Delete(string logKey)
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
            return
                "Message: " + exception.Message + "\r\n\r\n" +
                "Source: " + exception.Source + "\r\n\r\n" +
                "StackTrace: " + exception.StackTrace + "\r\n\r\n" +
                "TargetSite: " + exception.TargetSite + "\r\n\r\n" +
                "InnerException: " + exception.InnerException;

        }

        #region Write Logs

        public static void WriteError(string source, string text)
        {
            if (Config.LOGS_ERRORS_IN_CONSOLE)
            {
                Console(text);
            }
            Write(LogKeyError, source, text);
        }

        public static void WriteError(string source, string text, Exception exception)
        {
            string textError = GetText(text, exception);
            WriteError(source, textError);
        }

        public static void WriteError(string source, Exception exception)
        {
            string textError = GetText(exception);
            WriteError(source, textError);
        }

        public static void WriteError(string source, Uri uri, Exception exception)
        {
            source += " " + uri.ToString();
            WriteError(source, exception);
        }

        public static void WriteError(Uri uri, Exception exception)
        {
            string source = uri.ToString();
            WriteError(source, exception);
        }

        public static void WriteInfo(string source, string text)
        {
            if (Config.LOGS_INFO_IN_CONSOLE)
            {
                Console(text);
            }
            Write(LogKeyInfo, source, text);
        }

        public static void WriteInfo(Exception exception)
        {
            WriteInfo(string.Empty, exception);
        }

        public static void WriteInfo(string source, Exception exception)
        {
            string textError = GetText(exception);
            WriteInfo(source, textError);
        }

        public static void WriteBatch(string source, string text)
        {
            if (Config.LOGS_INFO_IN_CONSOLE)
            {
                Console(text);
            }
            Write(LogKeyBatch, source, text);
        }

        #endregion Write Logs

        public static bool Delete()
        {
            string query =
                "DELETE FROM " + TABLE_LOGS;

            return new DataBase().Query(query);
        }

        public static bool CleanTable()
        {
            string query =
                "DELETE FROM " + TABLE_LOGS + " " +
                "WHERE [Date] < DATEADD(YEAR, -1, GETDATE())";

            return new DataBase().Query(query);
        }
    }
}
