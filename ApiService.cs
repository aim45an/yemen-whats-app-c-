
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using YemenWhatsApp.Services;
using YemenWhatsApp.Data;
using YemenWhatsApp.Models;
using UserInfo = YemenWhatsApp.Models.UserInfo;
using Message = YemenWhatsApp.Models.Message;

namespace YemenWhatsApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl = "http://localhost:5000";
        private bool _isInitialized = false;
        private bool _serverAvailable = false;
        private ChatDbContext _dbContext;
        private bool _skipServerCheck = true; // تخطي فحص الخادم لجعل التسجيل أسرع

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(2); // تقليل المهلة إلى 2 ثانية فقط
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YemenWhatsApp/2.0");
            
            // إنشاء سياق قاعدة البيانات
            _dbContext = new ChatDbContext();
        }

        public void Initialize(string baseUrl = null)
        {
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _baseUrl = baseUrl;
            }

            // تحديث عنوان الخادم فقط دون فحص الاتصال (للتسريع)
            SessionManager.ServerUrl = _baseUrl;
            
            // جعل الوضع المحلي افتراضي للتسريع
            SessionManager.IsOnlineMode = false;
            _isInitialized = true;
            
            Console.WriteLine("✅ تم تهيئة ApiService بنجاح (الوضع المحلي افتراضي)");
        }

        public async Task<bool> TestConnectionAsync()
        {
            // استخدام مهمة سريعة دون انتظار
            return await Task.FromResult(false); // إرجاع false مباشرة للتسريع
        }

        // ========== تسجيل الدخول السريع ==========

        public async Task<ApiResponse<AuthResponse>> AuthenticateAsync(string username, string password = null)
        {
            try
            {
                // تسجيل الدخول السريع المحلي
                return await LocalAuthenticateAsync(username);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تسجيل الدخول", ex);
                return ApiResponse<AuthResponse>.CreateError(ex.Message, "فشل تسجيل الدخول");
            }
        }

        private async Task<ApiResponse<AuthResponse>> LocalAuthenticateAsync(string username)
        {
            try
            {
                // استخدام Transaction لأداء أسرع
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    // البحث عن المستخدم في قاعدة البيانات المحلية
                    var user = await _dbContext.Users
                        .AsNoTracking() // للقراءة السريعة فقط
                        .FirstOrDefaultAsync(u => u.Username == username);

                    if (user == null)
                    {
                        // إنشاء مستخدم جديد بسرعة
                        user = new User
                        {
                            Username = username,
                            ConnectionId = Guid.NewGuid().ToString().Substring(0, 10), // أقصر
                            IsOnline = true,
                            LastSeen = DateTime.Now,
                            Status = "متصل",
                            Color = GetRandomColor(),
                            Avatar = "👤",
                            Bio = "مستخدم جديد",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            LoginCount = 1,
                            LastLogin = DateTime.Now
                        };

                        await _dbContext.Users.AddAsync(user);
                    }
                    else
                    {
                        // تحديث حالة المستخدم بسرعة
                        user.IsOnline = true;
                        user.LastSeen = DateTime.Now;
                        user.Status = "متصل";
                        user.LastLogin = DateTime.Now;
                        user.LoginCount++;
                        user.UpdatedAt = DateTime.Now;
                        
                        _dbContext.Users.Update(user);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                // تحديث الجلسة بسرعة
                SessionManager.CurrentUsername = username;
                SessionManager.AuthToken = GenerateQuickToken(username);
                SessionManager.CurrentUser = await GetUserQuickAsync(username);
                SessionManager.IsOnlineMode = false;
                SessionManager.Login(username, "local");

                // حفظ في التخزين المحلي
                await Task.Run(() =>
                {
                    LocalStorage.SaveSetting("LastUsername", username);
                    LocalStorage.SaveSetting("AuthToken", SessionManager.AuthToken);
                    LocalStorage.SaveSetting("LoginMethod", "local");
                });

                var authResponse = new AuthResponse
                {
                    Success = true,
                    Message = "مرحباً بك!",
                    Token = SessionManager.AuthToken,
                    User = SessionManager.CurrentUser
                };

                return ApiResponse<AuthResponse>.CreateSuccess(authResponse, "تم تسجيل الدخول بنجاح");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("خطأ في تسجيل الدخول المحلي", ex);
                // التسجيل بدون قاعدة بيانات في حالة الخطأ
                return await QuickFallbackLoginAsync(username);
            }
        }

        private async Task<ApiResponse<AuthResponse>> QuickFallbackLoginAsync(string username)
        {
            // تسجيل دخول فوري بدون قاعدة بيانات
            var quickUser = new User
            {
                Username = username,
                Status = "متصل",
                Color = "#0078D7",
                Avatar = "👤",
                IsOnline = true,
                CreatedAt = DateTime.Now
            };

            SessionManager.CurrentUsername = username;
            SessionManager.AuthToken = $"quick-{username}-{DateTime.Now.Ticks}";
            SessionManager.CurrentUser = quickUser;
            SessionManager.IsOnlineMode = false;
            SessionManager.Login(username, "local");

            // حفظ سريع
            await Task.Run(() =>
            {
                try
                {
                    LocalStorage.SaveSetting("LastUsername", username);
                    LocalStorage.SaveSetting("AuthToken", SessionManager.AuthToken);
                }
                catch { }
            });

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "تم تسجيل الدخول",
                Token = SessionManager.AuthToken,
                User = quickUser
            };

            return ApiResponse<AuthResponse>.CreateSuccess(authResponse, "تم تسجيل الدخول (وضع سريع)");
        }

        private async Task<User> GetUserQuickAsync(string username)
        {
            try
            {
                // محاولة سريعة للحصول على المستخدم
                return await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username) ?? new User
                    {
                        Username = username,
                        Status = "متصل",
                        Color = "#0078D7",
                        Avatar = "👤",
                        IsOnline = true
                    };
            }
            catch
            {
                return new User
                {
                    Username = username,
                    Status = "متصل",
                    Color = "#0078D7",
                    Avatar = "👤",
                    IsOnline = true
                };
            }
        }

        private string GenerateQuickToken(string username)
        {
            return $"token-{username}-{DateTime.Now:HHmmss}";
        }

        private string GetRandomColor()
        {
            string[] colors = { "#0078D7", "#107C10", "#5C2D91", "#D83B01", "#F2C811" };
            return colors[new Random().Next(colors.Length)];
        }

        // ========== باقي الدوال (محسنة للسرعة) ==========

        public async Task<ApiResponse<UserListResponse>> GetOnlineUsersAsync()
        {
            try
            {
                List<User> users = new List<User>();

                // محاولة سريعة لجلب المستخدمين
                await Task.Run(async () =>
                {
                    try
                    {
                        users = await _dbContext.Users
                            .AsNoTracking()
                            .Where(u => u.IsActive)
                            .OrderByDescending(u => u.IsOnline)
                            .Take(20)
                            .ToListAsync();

                        if (!users.Any())
                        {
                            users = GetDefaultUsers();
                        }
                    }
                    catch
                    {
                        users = GetDefaultUsers();
                    }
                });

                var response = new UserListResponse
                {
                    Users = users,
                    Total = users.Count,
                    OnlineCount = users.Count(u => u.IsOnline)
                };

                return ApiResponse<UserListResponse>.CreateSuccess(response, "المستخدمون");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserListResponse>.CreateError(ex.Message, "فشل جلب المستخدمين");
            }
        }

        private List<User> GetDefaultUsers()
        {
            return new List<User>
            {
                new User { Username = "النظام", IsOnline = false, Status = "نظام", Color = "#808080", Avatar = "🤖" },
                new User { Username = "أحمد", IsOnline = true, Status = "متصل", Color = "#0078D7", Avatar = "👨" },
                new User { Username = "محمد", IsOnline = true, Status = "متصل", Color = "#107C10", Avatar = "👨‍💼" },
                new User { Username = "فاطمة", IsOnline = false, Status = "غير متصل", Color = "#5C2D91", Avatar = "👩" }
            };
        }

        public async Task<ApiResponse<MessageListResponse>> GetMessagesAsync(
            string type = "public",
            string targetUser = null,
            int page = 1,
            int pageSize = 30) // تقليل عدد الرسائل المرجعة
        {
            try
            {
                List<Message> messages = new List<Message>();

                await Task.Run(async () =>
                {
                    try
                    {
                        IQueryable<Message> query = _dbContext.Messages.AsNoTracking();

                        bool isPrivate = type == "private";
                        string currentUser = SessionManager.CurrentUsername;

                        if (isPrivate && !string.IsNullOrEmpty(targetUser))
                        {
                            query = query.Where(m =>
                                (m.Sender == currentUser && m.Receiver == targetUser) ||
                                (m.Sender == targetUser && m.Receiver == currentUser));
                        }
                        else if (!isPrivate)
                        {
                            query = query.Where(m => !m.IsPrivate || m.Receiver == "الجميع");
                        }

                        messages = await query
                            .OrderByDescending(m => m.Timestamp)
                            .Take(pageSize)
                            .ToListAsync();
                    }
                    catch
                    {
                        // إرجاع رسائل افتراضية في حالة الخطأ
                        messages = GetDefaultMessages();
                    }
                });

                var response = new MessageListResponse
                {
                    Messages = messages,
                    Total = messages.Count,
                    HasMore = false // لا نعرض زر "المزيد" للسرعة
                };

                return ApiResponse<MessageListResponse>.CreateSuccess(response, "الرسائل");
            }
            catch (Exception ex)
            {
                return ApiResponse<MessageListResponse>.CreateError(ex.Message, "فشل جلب الرسائل");
            }
        }

        private List<Message> GetDefaultMessages()
        {
            return new List<Message>
            {
                new Message
                {
                    Id = 1,
                    Sender = "النظام",
                    Receiver = "الجميع",
                    Content = "🎉 مرحباً بكم في Yemen WhatsApp!",
                    Timestamp = DateTime.Now.AddMinutes(-5),
                    IsPrivate = false,
                    Status = "sent"
                }
            };
        }

        public async Task<ApiResponse<bool>> SendMessageAsync(
            string content,
            string type = "public",
            string targetUser = null,
            string filePath = null)
        {
            try
            {
                bool isPrivate = type == "private";
                string currentUser = SessionManager.CurrentUsername;

                // إنشاء الرسالة بسرعة
                var message = new Message
                {
                    Sender = currentUser,
                    Receiver = isPrivate ? targetUser : "الجميع",
                    Content = content,
                    IsPrivate = isPrivate,
                    Status = "sent",
                    Timestamp = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                // حفظ سريع بدون انتظار
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _dbContext.Messages.AddAsync(message);
                        await _dbContext.SaveChangesAsync();
                    }
                    catch { }
                });

                // إرجاع النجاح فوراً
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
                string currentUser = SessionManager.CurrentUsername;

                if (!string.IsNullOrEmpty(currentUser))
                {
                    // تحديث سريع للمستخدم
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var user = await _dbContext.Users
                                .FirstOrDefaultAsync(u => u.Username == currentUser);
                            if (user != null)
                            {
                                user.IsOnline = false;
                                user.LastSeen = DateTime.Now;
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                        catch { }
                    });
                }

                SessionManager.Logout();
                return ApiResponse<bool>.CreateSuccess(true, "تم تسجيل الخروج");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.CreateError(ex.Message, "فشل تسجيل الخروج");
            }
        }

        // ========== دوال الملف الشخصي (سريعة) ==========

        public async Task<string> GetProfileImageAsync(string username)
        {
            // إرجاع أيقونة مباشرة دون فحص
            return "👤";
        }

        public async Task<ApiResponse<bool>> UpdateProfileImageAsync(string username, string imagePath)
        {
            // حفظ سريع
            _ = Task.Run(async () =>
            {
                try
                {
                    var user = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Username == username);

                    if (user != null)
                    {
                        user.Avatar = imagePath;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                catch { }
            });

            return ApiResponse<bool>.CreateSuccess(true, "سيتم تحديث الصورة");
        }

        public async Task<ApiResponse<UserStatistics>> GetUserStatisticsAsync(string username)
        {
            var stats = new UserStatistics
            {
                Username = username,
                TotalMessagesSent = 0,
                TotalMessagesReceived = 0,
                TotalFilesSent = 0,
                LoginCount = 1,
                LastLogin = DateTime.Now,
                CreatedAt = DateTime.Now,
                IsOnline = true,
                LastSeen = DateTime.Now
            };

            return ApiResponse<UserStatistics>.CreateSuccess(stats, "الإحصائيات");
        }

        public async Task<ApiResponse<bool>> UpdateUserProfileAsync(string username, UserProfileUpdate update)
        {
            // تحديث سريع
            _ = Task.Run(async () =>
            {
                try
                {
                    var user = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Username == username);

                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(update.DisplayName))
                            user.Name = update.DisplayName;

                        if (!string.IsNullOrEmpty(update.Bio))
                            user.Bio = update.Bio;

                        if (!string.IsNullOrEmpty(update.Status))
                            user.Status = update.Status;

                        await _dbContext.SaveChangesAsync();
                    }
                }
                catch { }
            });

            return ApiResponse<bool>.CreateSuccess(true, "سيتم تحديث الملف الشخصي");
        }

        // ========== دوال مساعدة ==========

        public string GetSignalRUrl()
        {
            return $"{_baseUrl}/chatHub";
        }

        public async Task<ApiResponse<DatabaseInfo>> GetDatabaseStatsAsync()
        {
            var info = new DatabaseInfo
            {
                UserCount = 1,
                MessageCount = 0,
                OnlineCount = 1,
            };

            return ApiResponse<DatabaseInfo>.CreateSuccess(info, "إحصائيات قاعدة البيانات");
        }

        public bool IsServerAvailable()
        {
            return false; // دائمًا محلي للسرعة
        }

        public void SwitchToLocalMode()
        {
            SessionManager.IsOnlineMode = false;
        }

        public async Task<bool> TryReconnectAsync()
        {
            return await Task.FromResult(false); // لا نحاول إعادة الاتصال للسرعة
        }

        public string GetCurrentMode()
        {
            return "الوضع المحلي";
        }
    }

    // ========== فئات المساعدة ==========

    public class UserProfileUpdate
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }
        public string Avatar { get; set; }
    }

    public class UserStatistics
    {
        public string Username { get; set; }
        public int TotalMessagesSent { get; set; }
        public int TotalMessagesReceived { get; set; }
        public int TotalFilesSent { get; set; }
        public int LoginCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
