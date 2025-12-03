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

        public static TimeSpan SessionDuration => DateTime.Now - _sessionStartTime;

        public static void Initialize()
        {
            try
            {
                //  Õ„Ì· «·≈⁄œ«œ«  «·„Õ›ÊŸ…
                _serverUrl = LocalStorage.GetSetting("ServerUrl", "http://localhost:5000");
                _currentUsername = LocalStorage.GetSetting("LastUsername", "");
                _authToken = LocalStorage.GetSetting("AuthToken", "");
                _isOnlineMode = LocalStorage.GetSettingBool("IsOnlineMode", true);

                _sessionStartTime = DateTime.Now;
                _isAuthenticated = !string.IsNullOrEmpty(_authToken);

                ErrorHandler.LogInfo($" „  ÂÌ∆… «·Ã·”… ··„” Œœ„: {_currentUsername}");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘·  ÂÌ∆… «·Ã·”…", ex);
            }
        }

        public static void ClearSession()
        {
            _currentUsername = null;
            _authToken = null;
            _currentUser = null;
            _isAuthenticated = false;

            // ·« ‰„”Õ ServerUrl Ê IsOnlineMode ·√‰Â« ≈⁄œ«œ«  ⁄«„…

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

                ErrorHandler.LogInfo(" „ Õ›Ÿ «·Ã·”…");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("›‘· Õ›Ÿ «·Ã·”…", ex);
            }
        }

        public static Dictionary<string, object> GetSessionInfo()
        {
            return new Dictionary<string, object>
            {
                ["Username"] = _currentUsername ?? "€Ì— „⁄—Ê›",
                ["IsAuthenticated"] = _isAuthenticated,
                ["SessionDuration"] = SessionDuration.ToString(@"hh\:mm\:ss"),
                ["ServerUrl"] = _serverUrl,
                ["IsOnlineMode"] = _isOnlineMode,
                ["SessionStart"] = _sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["UserStatus"] = _currentUser?.Status ?? "€Ì— „⁄—Ê›"
            };
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

        private static void OnSessionChanged()
        {
            SessionChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnUserChanged()
        {
            UserChanged?.Invoke(null, EventArgs.Empty);
        }

        // ÿ—ﬁ „”«⁄œ…
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

        public static string GenerateGuestUsername()
        {
            return $"÷Ì›_{new Random().Next(1000, 9999)}";
        }

        public static void LogSessionActivity(string activity)
        {
            ErrorHandler.LogInfo($"‰‘«ÿ «·Ã·”… [{_currentUsername}]: {activity}");
        }
    }
}