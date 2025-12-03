using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using YemenWhatsApp.Services;
using YemenWhatsApp.Data;
using YemenWhatsApp.Models;

namespace YemenWhatsApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5000";
        private bool _isInitialized = false;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YemenWhatsApp/2.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "ar-YE");
        }

        public void Initialize(string baseUrl = null)
        {
          

            // تحديث عنوان الخادم في SessionManager
            SessionManager.ServerUrl = _baseUrl;
            _isInitialized = true;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApiResponse<AuthResponse>> AuthenticateAsync(string username, string password = null)
        {
            try
            {
                // في هذا الإصدار، نستخدم نظام تسجيل دخول مبسط
                // يمكنك تعديله ليتصل بخادم حقيقي

                var response = new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "تم تسجيل الدخول بنجاح",
                    Data = new AuthResponse
                    {
                        Success = true,
                        Message = "مرحباً بك في Yemen WhatsApp",
                        Token = GenerateToken(username),
                        User = new User
                        {
                            Username = username,
                            Status = "متصل",
                            Color = "#0078D7",
                            Avatar = "👤",
                            IsOnline = true
                        }
                    }
                };

                // تحديث الجلسة
                SessionManager.CurrentUsername = username;
                SessionManager.AuthToken = response.Data.Token;
                SessionManager.CurrentUser = response.Data.User;

                // حفظ في التخزين المحلي
                LocalStorage.SaveSetting("LastUsername", username);
                LocalStorage.SaveSetting("AuthToken", response.Data.Token);

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.CreateError(ex.Message, "فشل تسجيل الدخول");
            }
        }

        public async Task<ApiResponse<UserListResponse>> GetOnlineUsersAsync()
        {
            try
            {
                // محاكاة للحصول على المستخدمين
                var users = new List<User>
                {
                    new User { Username = "أحمد", IsOnline = true, Status = "متصل", Color = "#0078D7" },
                    new User { Username = "محمد", IsOnline = true, Status = "متصل", Color = "#107C10" },
                    new User { Username = "فاطمة", IsOnline = true, Status = "متصل", Color = "#5C2D91" },
                    new User { Username = "خالد", IsOnline = false, Status = "غير متصل", Color = "#D83B01" },
                    new User { Username = "سارة", IsOnline = true, Status = "متصل", Color = "#F2C811" }
                };

                // إضافة المستخدمين من قاعدة البيانات المحلية
                try
                {
                    using (var db = new Data.ChatDbContext())
                    {
                        var dbUsers = await db.GetOnlineUsersAsync();
                        users.AddRange(dbUsers.Where(u => !users.Any(x => x.Username == u.Username)));
                    }
                }
                catch { }

                var response = new UserListResponse
                {
                    Users = users,
                    Total = users.Count,
                    OnlineCount = users.Count(u => u.IsOnline)
                };

                return ApiResponse<UserListResponse>.CreateSuccess(response, "تم جلب المستخدمين");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserListResponse>.CreateError(ex.Message, "فشل جلب المستخدمين");
            }
        }

        public async Task<ApiResponse<MessageListResponse>> GetMessagesAsync(string type = "public", string targetUser = null, int page = 1, int pageSize = 50)
        {
            try
            {
                List<Models.Message> messages = new List<Models.Message>();

                // جلب الرسائل من قاعدة البيانات المحلية
                using (var db = new Data.ChatDbContext())
                {
                    bool isPrivate = type == "private";
                    string currentUser = SessionManager.CurrentUsername;

                    messages = await db.GetRecentMessagesAsync(100, isPrivate, currentUser, targetUser);
                }

                var response = new MessageListResponse
                {
                    Messages = messages,
                    Total = messages.Count,
                    HasMore = messages.Count >= pageSize
                };

                return ApiResponse<MessageListResponse>.CreateSuccess(response, "تم جلب الرسائل");
            }
            catch (Exception ex)
            {
                return ApiResponse<MessageListResponse>.CreateError(ex.Message, "فشل جلب الرسائل");
            }
        }

        public async Task<ApiResponse<bool>> SendMessageAsync(string content, string type = "public", string targetUser = null, string filePath = null)
        {
            try
            {
                bool isPrivate = type == "private";

                // حفظ في قاعدة البيانات المحلية
                using (var db = new Data.ChatDbContext())
                {
                    var message = new Models.Message
                    {
                        Sender = SessionManager.CurrentUsername,
                        Receiver = isPrivate ? targetUser : "الجميع",
                        Content = content,
                        IsPrivate = isPrivate,
                        Status = "sent",
                        Timestamp = DateTime.Now
                    };

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        message.MessageType = GetMessageType(filePath);
                        message.FilePath = filePath;
                        message.FileName = Path.GetFileName(filePath);
                        message.FileSize = new FileInfo(filePath).Length;
                    }

                    await db.Messages.AddAsync(message);
                    await db.SaveChangesAsync();
                }

                // محاولة الإرسال للخادم إذا كان متصلاً
                if (SessionManager.IsOnlineMode && _isInitialized)
                {
                    try
                    {
                        var payload = new
                        {
                            content,
                            type,
                            targetUser,
                            sender = SessionManager.CurrentUsername,
                            timestamp = DateTime.Now
                        };

                        var json = JsonConvert.SerializeObject(payload);
                        var contentData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync($"{_baseUrl}/api/messages/send", contentData);

                        if (!response.IsSuccessStatusCode)
                        {
                            ErrorHandler.LogError($"فشل إرسال الرسالة للخادم: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError("خطأ في إرسال الرسالة للخادم", ex);
                    }
                }

                return ApiResponse<bool>.CreateSuccess(true, "تم إرسال الرسالة");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.CreateError(ex.Message, "فشل إرسال الرسالة");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                // تحديث حالة المستخدم في قاعدة البيانات المحلية
                using (var db = new Data.ChatDbContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == SessionManager.CurrentUsername);
                    if (user != null)
                    {
                        user.IsOnline = false;
                        user.LastSeen = DateTime.Now;
                        user.Status = "غير متصل";
                        await db.SaveChangesAsync();
                    }
                }

                // إرسال طلب تسجيل خروج للخادم
                if (SessionManager.IsOnlineMode && _isInitialized)
                {
                    try
                    {
                        await _httpClient.PostAsync($"{_baseUrl}/api/auth/logout", null);
                    }
                    catch { }
                }

                // مسح الجلسة
                SessionManager.ClearSession();
                LocalStorage.RemoveSetting("AuthToken");

                return ApiResponse<bool>.CreateSuccess(true, "تم تسجيل الخروج");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.CreateError(ex.Message, "فشل تسجيل الخروج");
            }
        }

        public async Task<ApiResponse<DatabaseInfo>> GetDatabaseStatsAsync()
        {
            try
            {
                var info = await Data.DatabaseHelper.GetDatabaseInfo();
                return ApiResponse<DatabaseInfo>.CreateSuccess(info, "إحصائيات قاعدة البيانات");
            }
            catch (Exception ex)
            {
                return ApiResponse<DatabaseInfo>.CreateError(ex.Message, "فشل جلب الإحصائيات");
            }
        }

        // دوال مساعدة
        private string GenerateToken(string username)
        {
            return $"yemen-whatsapp-token-{username}-{DateTime.Now.Ticks}-{Guid.NewGuid():N}";
        }

        private string GetMessageType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "image",
                ".mp3" or ".wav" or ".ogg" => "audio",
                ".mp4" or ".avi" or ".mov" => "video",
                ".pdf" or ".doc" or ".docx" or ".txt" => "document",
                _ => "file"
            };
        }

        // طريقة للاتصال بخادم SignalR
        public string GetSignalRUrl()
        {
            return $"{_baseUrl}/chatHub";
        }
    }
}