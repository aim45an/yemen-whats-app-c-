using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace YemenWhatsApp.Services
{
    public static class ErrorHandler
    {
        private static readonly string _logFilePath;
        private static readonly object _lock = new object();

        static ErrorHandler()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "YemenWhatsApp");
                string logsFolder = Path.Combine(appFolder, "Logs");

                if (!Directory.Exists(logsFolder))
                {
                    Directory.CreateDirectory(logsFolder);
                }

                _logFilePath = Path.Combine(logsFolder, $"app_log_{DateTime.Now:yyyyMMdd}.txt");
            }
            catch
            {
                _logFilePath = Path.Combine(Path.GetTempPath(), "YemenWhatsApp_log.txt");
            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            lock (_lock)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {message}";

                    if (ex != null)
                    {
                        logEntry += $"\nException: {ex.GetType().Name}";
                        logEntry += $"\nMessage: {ex.Message}";
                        logEntry += $"\nStackTrace: {ex.StackTrace}";

                        if (ex.InnerException != null)
                        {
                            logEntry += $"\nInner Exception: {ex.InnerException.Message}";
                        }
                    }

                    logEntry += "\n" + new string('-', 80) + "\n";

                    File.AppendAllText(_logFilePath, logEntry);
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to log error: {logEx.Message}");
                }
            }
        }

        public static void LogInfo(string message)
        {
            lock (_lock)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}\n";
                    File.AppendAllText(_logFilePath, logEntry);
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
                catch { }
            }
        }

        public static void LogWarning(string message)
        {
            lock (_lock)
            {
                try
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARNING] {message}\n";
                    File.AppendAllText(_logFilePath, logEntry);
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
                catch { }
            }
        }

        public static void ShowError(string title, string message, Exception ex = null)
        {
            string fullMessage = message;

            if (ex != null)
            {
                fullMessage += $"\n\n ›«’Ì· «·Œÿ√: {ex.Message}";

#if DEBUG
                fullMessage += $"\n\nStack Trace:\n{ex.StackTrace}";
#endif
            }

            MessageBox.Show(fullMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

            LogError(message, ex);
        }

        public static void ShowWarning(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            LogWarning(message);
        }

        public static void ShowInfo(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            LogInfo(message);
        }

        public static DialogResult ShowConfirm(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
        {
            var result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question);
            LogInfo($"User confirmed: {result} for '{title}'");
            return result;
        }

        public static string GetLogContent()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    return File.ReadAllText(_logFilePath);
                }
                return "·«  ÊÃœ ”Ã·«  „ «Õ…";
            }
            catch (Exception ex)
            {
                return $"›‘· ﬁ—«¡… «·”Ã·« : {ex.Message}";
            }
        }

        public static void ClearLogs(int daysToKeep = 7)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logsFolder = Path.Combine(appDataPath, "YemenWhatsApp", "Logs");

                if (Directory.Exists(logsFolder))
                {
                    var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                    foreach (var file in Directory.GetFiles(logsFolder, "app_log_*.txt"))
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            File.Delete(file);
                        }
                    }

                    LogInfo($" „  ‰ŸÌ› «·”Ã·«  «·√ﬁœ„ „‰ {daysToKeep} ÌÊ„");
                }
            }
            catch (Exception ex)
            {
                LogError("›‘·  ‰ŸÌ› «·”Ã·« ", ex);
            }
        }

        public static string GetLogFilePath()
        {
            return _logFilePath;
        }

        public static void LogObject(string message, object obj)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                LogInfo($"{message}:\n{json}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to log object: {message}", ex);
            }
        }
    }
}