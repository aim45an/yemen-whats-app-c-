using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace YemenWhatsApp.Services
{
    public static class LocalStorage
    {
        private static readonly string _settingsFilePath;
        private static Dictionary<string, string> _settings;
        private static readonly object _lock = new object();

        static LocalStorage()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "YemenWhatsApp");
                string dataFolder = Path.Combine(appFolder, "Data");

                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                _settingsFilePath = Path.Combine(dataFolder, "settings.json");
                LoadSettings();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘·  ÂÌ∆… «· Œ“Ì‰ «·„Õ·Ì", ex);
                _settings = new Dictionary<string, string>();
            }
        }

        private static void LoadSettings()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_settingsFilePath))
                    {
                        string json = File.ReadAllText(_settingsFilePath);
                        _settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                            ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        _settings = new Dictionary<string, string>();
                        SaveSettings(); // ≈‰‘«¡ „·› ÃœÌœ
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("›‘·  Õ„Ì· «·≈⁄œ«œ« ", ex);
                    _settings = new Dictionary<string, string>();
                }
            }
        }

        private static void SaveSettings()
        {
            lock (_lock)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                    File.WriteAllText(_settingsFilePath, json);
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("›‘· Õ›Ÿ «·≈⁄œ«œ« ", ex);
                }
            }
        }

        public static void SaveSetting(string key, string value)
        {
            lock (_lock)
            {
                _settings[key] = value;
                SaveSettings();
            }
        }

        public static string GetSetting(string key, string defaultValue = "")
        {
            lock (_lock)
            {
                if (_settings.TryGetValue(key, out string value))
                {
                    return value;
                }
                return defaultValue;
            }
        }

        public static bool GetSettingBool(string key, bool defaultValue = false)
        {
            string value = GetSetting(key, defaultValue.ToString());
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        public static int GetSettingInt(string key, int defaultValue = 0)
        {
            string value = GetSetting(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static void RemoveSetting(string key)
        {
            lock (_lock)
            {
                if (_settings.ContainsKey(key))
                {
                    _settings.Remove(key);
                    SaveSettings();
                }
            }
        }

        public static void ClearAllSettings()
        {
            lock (_lock)
            {
                _settings.Clear();
                SaveSettings();
            }
        }

        public static Dictionary<string, string> GetAllSettings()
        {
            lock (_lock)
            {
                return new Dictionary<string, string>(_settings);
            }
        }

        public static bool SettingExists(string key)
        {
            lock (_lock)
            {
                return _settings.ContainsKey(key);
            }
        }

        // ÿ—ﬁ „”«⁄œ… ·· Œ“Ì‰ «·„⁄ﬁœ
        public static void SaveObject<T>(string key, T obj)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
                SaveSetting(key, json);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"›‘· Õ›Ÿ «·ﬂ«∆‰: {key}", ex);
            }
        }

        public static T GetObject<T>(string key, T defaultValue = default)
        {
            try
            {
                string json = GetSetting(key);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"›‘·  Õ„Ì· «·ﬂ«∆‰: {key}", ex);
            }

            return defaultValue;
        }

        // ÿ—ﬁ ·≈œ«—… „·›«  «·„” Œœ„
        public static string GetUserDataPath(string username)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userFolder = Path.Combine(appDataPath, "YemenWhatsApp", "Users", username);

            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            return userFolder;
        }

        public static string GetMediaPath(string username, string mediaType = "photos")
        {
            string userFolder = GetUserDataPath(username);
            string mediaFolder = Path.Combine(userFolder, mediaType);

            if (!Directory.Exists(mediaFolder))
            {
                Directory.CreateDirectory(mediaFolder);
            }

            return mediaFolder;
        }

        public static string SaveFile(string username, string sourceFilePath, string mediaType = "files")
        {
            try
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string destFolder = GetMediaPath(username, mediaType);
                string destPath = Path.Combine(destFolder, $"{Guid.NewGuid()}_{fileName}");

                File.Copy(sourceFilePath, destPath, true);
                return destPath;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"›‘· Õ›Ÿ «·„·›: {sourceFilePath}", ex);
                return null;
            }
        }

        public static List<string> GetUserFiles(string username, string mediaType = "files")
        {
            try
            {
                string mediaFolder = GetMediaPath(username, mediaType);
                return Directory.GetFiles(mediaFolder).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // ‰”Œ «Õ Ì«ÿÌ ··≈⁄œ«œ« 
        public static void BackupSettings()
        {
            try
            {
                string backupPath = _settingsFilePath + $".backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(_settingsFilePath, backupPath, true);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘· ≈‰‘«¡ ‰”Œ… «Õ Ì«ÿÌ… ··≈⁄œ«œ« ", ex);
            }
        }

        public static void RestoreSettings(string backupFilePath)
        {
            try
            {
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, _settingsFilePath, true);
                    LoadSettings(); // ≈⁄«œ…  Õ„Ì· «·≈⁄œ«œ« 
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘· «” ⁄«œ… «·≈⁄œ«œ« ", ex);
            }
        }

        //  ‰ŸÌ› «·„·›«  «·ﬁœÌ„…
        public static void CleanupOldFiles(int daysToKeep = 30)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "YemenWhatsApp");

                if (Directory.Exists(appFolder))
                {
                    CleanupDirectory(appFolder, daysToKeep);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘·  ‰ŸÌ› «·„·›«  «·ﬁœÌ„…", ex);
            }
        }

        private static void CleanupDirectory(string directory, int daysToKeep)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

            foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".json") || file.EndsWith(".txt") || file.EndsWith(".log"))
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}