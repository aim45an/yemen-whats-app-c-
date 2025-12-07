using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YemenWhatsApp.Services;
using YemenWhatsApp.Models;

namespace YemenWhatsApp.Data
{
    public static class DatabaseHelper
    {
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        public static void InitializeDatabase()
        {
            lock (_lock)
            {
                if (_isInitialized) return;

                try
                {
                    using (var context = new ChatDbContext())
                    {
                        // اختبار الاتصال بقاعدة البيانات
                        bool canConnect = context.Database.CanConnect();

                        if (!canConnect)
                        {
                            ErrorHandler.LogError("❌ لا يمكن الاتصال بقاعدة البيانات");

                            // محاولة إنشاء قاعدة البيانات
                            try
                            {
                                context.Database.EnsureCreated();
                                ErrorHandler.LogInfo("✅ تم إنشاء قاعدة البيانات");
                            }
                            catch (Exception createEx)
                            {
                                ErrorHandler.LogError("❌ فشل إنشاء قاعدة البيانات", createEx);
                                throw new Exception($"فشل إنشاء قاعدة البيانات: {createEx.Message}");
                            }
                        }
                        else
                        {
                            ErrorHandler.LogInfo("✅ تم الاتصال بقاعدة البيانات");
                        }

                        // التحقق من وجود الجداول وإنشاؤها إذا لزم الأمر
                        try
                        {
                            context.Database.ExecuteSqlRaw(@"
                                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                                BEGIN
                                    CREATE TABLE [dbo].[Users] (
                                        [Id] [int] IDENTITY(1,1) NOT NULL,
                                        [Username] [nvarchar](100) NOT NULL,
                                        [Password] [nvarchar](500) NOT NULL,
                                        [Email] [nvarchar](100) NULL,
                                        [ConnectionId] [nvarchar](500) NULL,
                                        [IsOnline] [bit] NOT NULL DEFAULT 0,
                                        [LastSeen] [datetime2](7) NOT NULL,
                                        [Status] [nvarchar](100) NULL,
                                        [Color] [nvarchar](50) NULL,
                                        [Avatar] [nvarchar](20) NULL,
                                        [Bio] [nvarchar](500) NULL,
                                        [ProfileImagePath] [nvarchar](500) NULL,
                                        [ProfileThumbnailPath] [nvarchar](500) NULL,
                                        [ProfileImageUpdatedAt] [datetime2](7) NULL,
                                        [FailedLoginAttempts] [int] NOT NULL DEFAULT 0,
                                        [LastFailedLogin] [datetime2](7) NULL,
                                        [IsLocked] [bit] NOT NULL DEFAULT 0,
                                        [LockedUntil] [datetime2](7) NULL,
                                        [PhoneNumber] [nvarchar](20) NULL,
                                        [IsVerified] [bit] NOT NULL DEFAULT 0,
                                        [CreatedAt] [datetime2](7) NOT NULL,
                                        [UpdatedAt] [datetime2](7) NOT NULL,
                                        [LastActivity] [datetime2](7) NULL,
                                        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
                                    );
                                    CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users] ([Username]);
                                    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]) WHERE [Email] IS NOT NULL;
                                    CREATE INDEX [IX_Users_IsOnline] ON [dbo].[Users] ([IsOnline]);
                                    CREATE INDEX [IX_Users_LastSeen] ON [dbo].[Users] ([LastSeen]);
                                END
                            ");

                            context.Database.ExecuteSqlRaw(@"
                                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Messages' AND xtype='U')
                                BEGIN
                                    CREATE TABLE [dbo].[Messages] (
                                        [Id] [int] IDENTITY(1,1) NOT NULL,
                                        [Sender] [nvarchar](100) NOT NULL,
                                        [Receiver] [nvarchar](100) NOT NULL,
                                        [Content] [nvarchar](MAX) NOT NULL,
                                        [MessageType] [nvarchar](50) NULL,
                                        [FilePath] [nvarchar](500) NULL,
                                        [FileName] [nvarchar](500) NULL,
                                        [FileSize] [bigint] NULL,
                                        [IsPrivate] [bit] NOT NULL,
                                        [IsRead] [bit] NOT NULL,
                                        [Timestamp] [datetime2](7) NOT NULL,
                                        [Status] [nvarchar](50) NULL,
                                        [CreatedAt] [datetime2](7) NOT NULL,
                                        [SenderId] [int] NULL,
                                        [ReceiverId] [int] NULL,
                                        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id])
                                    );
                                    CREATE INDEX [IX_Messages_Sender] ON [dbo].[Messages] ([Sender]);
                                    CREATE INDEX [IX_Messages_Receiver] ON [dbo].[Messages] ([Receiver]);
                                    CREATE INDEX [IX_Messages_Timestamp] ON [dbo].[Messages] ([Timestamp]);
                                    CREATE INDEX [IX_Messages_IsPrivate] ON [dbo].[Messages] ([IsPrivate]);
                                    CREATE INDEX [IX_Messages_Sender_Receiver_Timestamp] ON [dbo].[Messages] ([Sender], [Receiver], [Timestamp]);
                                    CREATE INDEX [IX_Messages_Receiver_Sender_Timestamp] ON [dbo].[Messages] ([Receiver], [Sender], [Timestamp]);
                                END
                            ");
                        }
                        catch (Exception sqlEx)
                        {
                            ErrorHandler.LogError("❌ فشل إنشاء الجداول", sqlEx);
                        }

                        // إضافة البيانات الأولية
                        SeedInitialData(context);

                        _isInitialized = true;
                        ErrorHandler.LogInfo("✅ تم تهيئة قاعدة البيانات بنجاح");
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("❌ فشل تهيئة قاعدة البيانات", ex);
                    throw new Exception($"فشل تهيئة قاعدة البيانات: {ex.Message}");
                }
            }
        }

        private static void SeedInitialData(ChatDbContext context)
        {
            try
            {
                // التحقق من وجود المستخدم النظامي
                if (!context.Users.Any(u => u.Username == "النظام"))
                {
                    var systemUser = new User
                    {
                        Username = "النظام",
                        Password = HashPassword("system123"),
                        Status = "نظام",
                        IsOnline = false,
                        Color = "#808080",
                        Avatar = "🤖",
                        Bio = "حساب النظام - Yemen WhatsApp",
                        IsVerified = true
                    };
                    context.Users.Add(systemUser);
                    context.SaveChanges();
                }

                // التحقق من وجود رسالة ترحيبية
                if (!context.Messages.Any())
                {
                    var welcomeMessage = new Models.Message
                    {
                        Sender = "النظام",
                        Receiver = "الجميع",
                        Content = "🎉 مرحباً بكم في Yemen WhatsApp Desktop! 💬\n\nيمكنكم الآن الدردشة مع الأصدقاء والزملاء بشكل آمن وسريع.\n\n🇾🇪 تطوير يمني ١٠٠٪",
                        IsPrivate = false,
                        Status = "sent",
                        Timestamp = DateTime.Now.AddMinutes(-5),
                        CreatedAt = DateTime.Now
                    };
                    context.Messages.Add(welcomeMessage);
                    context.SaveChanges();
                }

                ErrorHandler.LogInfo("✅ تم إضافة البيانات الأولية");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("❌ فشل إضافة البيانات الأولية", ex);
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static async Task BackupDatabase(string backupPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(backupPath))
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    backupPath = Path.Combine(appData, "YemenWhatsApp", "Backups",
                        $"YemenChatDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                }

                string backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                using (var context = new ChatDbContext())
                {
                    var backupQuery = $@"
                        BACKUP DATABASE YemenChatDB 
                        TO DISK = '{backupPath.Replace("'", "''")}'
                        WITH FORMAT, 
                             MEDIANAME = 'YemenChatBackup',
                             NAME = 'Full Backup of YemenChatDB';
                    ";

                    await context.Database.ExecuteSqlRawAsync(backupQuery);
                    ErrorHandler.LogInfo($"✅ تم إنشاء نسخة احتياطية في: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("❌ فشل إنشاء نسخة احتياطية", ex);
                throw;
            }
        }

        public static async Task<DatabaseInfo> GetDatabaseInfo()
        {
            using (var context = new ChatDbContext())
            {
                var info = new DatabaseInfo();

                try
                {
                    info.UserCount = await context.Users.CountAsync();
                    info.MessageCount = await context.Messages.CountAsync();
                    info.OnlineUsers = await context.Users.CountAsync(u => u.IsOnline);
                    info.LastMessageTime = await context.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Timestamp)
                        .FirstOrDefaultAsync();

                    info.DatabaseSize = await GetDatabaseSize(context);
                    info.IsConnected = context.Database.CanConnect();
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError("❌ فشل جلب معلومات قاعدة البيانات", ex);
                }

                return info;
            }
        }

        private static async Task<long> GetDatabaseSize(ChatDbContext context)
        {
            try
            {
                var result = await context.Database
                    .SqlQueryRaw<long>("SELECT SUM(size) * 8 * 1024 FROM sys.database_files")
                    .FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                return 0;
            }
        }

        public static async Task CleanupOldMessages(int daysToKeep = 30)
        {
            try
            {
                using (var context = new ChatDbContext())
                {
                    var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                    var oldMessages = await context.Messages
                        .Where(m => m.Timestamp < cutoffDate && !m.IsPrivate)
                        .ToListAsync();

                    if (oldMessages.Any())
                    {
                        context.Messages.RemoveRange(oldMessages);
                        await context.SaveChangesAsync();
                        ErrorHandler.LogInfo($"✅ تم حذف {oldMessages.Count} رسالة أقدم من {daysToKeep} يوم");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("❌ فشل تنظيف الرسائل القديمة", ex);
            }
        }
    }

    public class DatabaseInfo
    {
        internal int OnlineCount;

        public int UserCount { get; set; }
        public int MessageCount { get; set; }
        public int OnlineUsers { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public long DatabaseSize { get; set; }
        public bool IsConnected { get; set; }

        public string DatabaseSizeFormatted
        {
            get
            {
                if (DatabaseSize < 1024) return $"{DatabaseSize} بايت";
                if (DatabaseSize < 1024 * 1024) return $"{DatabaseSize / 1024:F1} ك.بايت";
                if (DatabaseSize < 1024 * 1024 * 1024) return $"{DatabaseSize / (1024 * 1024):F1} م.بايت";
                return $"{DatabaseSize / (1024 * 1024 * 1024):F1} ج.بايت";
            }
        }

        public override string ToString()
        {
            return $"المستخدمون: {UserCount} | الرسائل: {MessageCount} | متصلون: {OnlineUsers} | الحجم: {DatabaseSizeFormatted}";
        }
    }
}