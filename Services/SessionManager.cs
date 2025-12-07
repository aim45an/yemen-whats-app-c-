using System;
using System.Collections.Generic;
using YemenWhatsApp.Models;

namespace YemenWhatsApp.Services
{
    public static class SessionManager
    {
        private static string _currentUsername;
        private static string _serverUrl = "http://localhost:5000";
        private static bool _isOnlineMode = true;
        private static string _authToken;
        private static User _currentUser;
        private static DateTime _sessionStartTime;
        private static bool _isAuthenticated = false;
        private static bool _isGuest = false;
        private static string _loginMethod = "local";

        public static event EventHandler SessionChanged;
        public static event EventHandler UserChanged;

        public static string CurrentUsername
        {
            get => _currentUsername;
            set
            {
                if (_currentUsername != value)
                {
                    _currentUsername = value;
                    OnSessionChanged();
                }
            }
        }

        public static string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl != value)
                {
                    _serverUrl = value;
                    LocalStorage.SaveSetting("ServerUrl", value);
                    OnSessionChanged();
                }
            }
        }

        public static bool IsOnlineMode
        {
            get => _isOnlineMode;
            set
            {
                if (_isOnlineMode != value)
                {
                    _isOnlineMode = value;
                    LocalStorage.SaveSetting("IsOnlineMode", value.ToString());
                    OnSessionChanged();
                }
            }
        }

        public static string AuthToken
        {
            get => _authToken;
            set
            {
                if (_authToken != value)
                {
                    _authToken = value;
                    _isAuthenticated = !string.IsNullOrEmpty(value);
                    OnSessionChanged();
                }
            }
        }

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnUserChanged();
                }
            }
        }

        public static DateTime SessionStartTime => _sessionStartTime;

        public static bool IsAuthenticated => _isAuthenticated && !string.IsNullOrEmpty(_currentUsername);

        public static bool IsGuest => _isGuest;

        public static string LoginMethod => _loginMethod;

        public static TimeSpan SessionDuration => DateTime.Now - _sessionStartTime;

        public static object Instance { get; internal set; }

        public static void Initialize()
        {
            try
            {
                _serverUrl = LocalStorage.GetSetting("ServerUrl", "http://localhost:5000");
                _currentUsername = LocalStorage.GetSetting("LastUsername", "");
                _authToken = LocalStorage.GetSetting("AuthToken", "");
                _isOnlineMode = LocalStorage.GetSettingBool("IsOnlineMode", true);
                _isGuest = LocalStorage.GetSettingBool("IsGuest", false);
                _loginMethod = LocalStorage.GetSetting("LoginMethod", "local");

                _sessionStartTime = DateTime.Now;
                _isAuthenticated = !string.IsNullOrEmpty(_authToken);

                ErrorHandler.LogInfo($" „  ÂÌ∆… «·Ã·”… ··„” Œœ„: {_currentUsername}");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("Œÿ√ ›Ì  ÂÌ∆… «·Ã·”…", ex);
            }
        }

        public static void ClearSession()
        {
            _currentUsername = null;
            _authToken = null;
            _currentUser = null;
            _isAuthenticated = false;
            _isGuest = false;

            LocalStorage.SaveSetting("IsLoggedIn", "false");
            OnSessionChanged();
            ErrorHandler.LogInfo(" „ „”Õ «·Ã·”…");
        }

        public static void SaveSession()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentUsername))
                {
                    LocalStorage.SaveSetting("LastUsername", _currentUsername);
                }

                if (!string.IsNullOrEmpty(_authToken))
                {
                    LocalStorage.SaveSetting("AuthToken", _authToken);
                }

                LocalStorage.SaveSetting("IsOnlineMode", _isOnlineMode.ToString());
                LocalStorage.SaveSetting("ServerUrl", _serverUrl);
                LocalStorage.SaveSetting("IsGuest", _isGuest.ToString());
                LocalStorage.SaveSetting("LoginMethod", _loginMethod);

                ErrorHandler.LogInfo(" „ Õ›Ÿ «·Ã·”…");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("Œÿ√ ›Ì Õ›Ÿ «·Ã·”…", ex);
            }
        }

        public static Dictionary<string, object> GetSessionInfo()
        {
            return new Dictionary<string, object>
            {
                ["Username"] = _currentUsername ?? "€Ì— „Õœœ",
                ["IsAuthenticated"] = _isAuthenticated,
                ["SessionDuration"] = SessionDuration.ToString(@"hh\:mm\:ss"),
                ["ServerUrl"] = _serverUrl,
                ["IsOnlineMode"] = _isOnlineMode,
                ["IsGuest"] = _isGuest,
                ["LoginMethod"] = _loginMethod,
                ["SessionStart"] = _sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["UserStatus"] = _currentUser?.Status ?? "€Ì— „Õœœ"
            };
        }

        public static void Login(string username, string method = "local", bool isGuest = false)
        {
            CurrentUsername = username;
            _isAuthenticated = true;
            _loginMethod = method;
            _isGuest = isGuest;
            _sessionStartTime = DateTime.Now;

            LocalStorage.SaveSetting("LoginMethod", method);
            LocalStorage.SaveSetting("IsGuest", isGuest.ToString());

            ErrorHandler.LogInfo($" „  ”ÃÌ· «·œŒÊ·: {username} (ÿ—Ìﬁ…: {method}, ÷Ì›: {isGuest})");
        }

        public static void Logout()
        {
            string oldUsername = _currentUsername;

            ClearSession();
            ErrorHandler.LogInfo($" „  ”ÃÌ· «·Œ—ÊÃ: {oldUsername}");
        }

        public static void UpdateUserStatus(string status)
        {
            if (_currentUser != null)
            {
                _currentUser.Status = status;
                OnUserChanged();
            }
        }

        public static void UpdateUserColor(string color)
        {
            if (_currentUser != null)
            {
                _currentUser.Color = color;
                OnUserChanged();
            }
        }

        public static void UpdateUserAvatar(string avatar)
        {
            if (_currentUser != null)
            {
                _currentUser.Avatar = avatar;
                OnUserChanged();
            }
        }

        public static string GenerateGuestUsername()
        {
            return $"÷Ì›_{new Random().Next(1000, 9999)}";
        }

        private static void OnSessionChanged()
        {
            SessionChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnUserChanged()
        {
            UserChanged?.Invoke(null, EventArgs.Empty);
        }

        public static bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length >= 3 &&
                   username.Length <= 50;
        }

        public static bool IsValidServerUrl(string url)
        {
            try
            {
                return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
            catch
            {
                return false;
            }
        }

        public static void LogSessionActivity(string activity)
        {
            ErrorHandler.LogInfo($"‰‘«ÿ «·Ã·”… [{_currentUsername}]: {activity}");
        }
    }
}