using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YemenWhatsApp.Models;
using YemenWhatsApp.Services;

namespace YemenWhatsApp.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public ChatDbContext() { }

        public DbSet<User> Users { get; set; }
        public DbSet<Models.Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // سلسلة الاتصال الافتراضية
                string connectionString = @"Server=DESKTOP-2U7RVGF;
                    Database=YemenChatDB;
                    Trusted_Connection=True;
                    TrustServerCertificate=True;";

                optionsBuilder.UseSqlServer(connectionString, options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                    options.CommandTimeout(30);
                });

#if DEBUG
                // فقط في وضع التطوير
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.LogTo(message =>
                    System.Diagnostics.Debug.WriteLine(message),
                    LogLevel.Information);
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تكوين جدول المستخدمين
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                entity.HasIndex(e => e.IsOnline)
                    .HasDatabaseName("IX_Users_IsOnline");

                entity.HasIndex(e => e.LastSeen)
                    .HasDatabaseName("IX_Users_LastSeen");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValue("متصل")
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.Color)
                    .HasMaxLength(50)
                    .HasDefaultValue("#0078D7");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasDefaultValue("👤");

                entity.Property(e => e.Bio)
                    .HasMaxLength(500)
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastSeen)
                    .HasDefaultValueSql("GETDATE()");
            });

            // تكوين جدول الرسائل
            modelBuilder.Entity<Models.Message>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Sender)
                    .HasDatabaseName("IX_Messages_Sender");

                entity.HasIndex(e => e.Receiver)
                    .HasDatabaseName("IX_Messages_Receiver");

                entity.HasIndex(e => e.IsPrivate)
                    .HasDatabaseName("IX_Messages_IsPrivate");

                entity.HasIndex(e => e.IsRead)
                    .HasDatabaseName("IX_Messages_IsRead");

                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("IX_Messages_Timestamp");

                entity.HasIndex(e => new { e.Sender, e.Receiver, e.Timestamp })
                    .HasDatabaseName("IX_Messages_Sender_Receiver_Timestamp");

                entity.HasIndex(e => new { e.Receiver, e.Sender, e.Timestamp })
                    .HasDatabaseName("IX_Messages_Receiver_Sender_Timestamp");

                entity.Property(e => e.Sender)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.Receiver)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("الجميع")
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnType("nvarchar(MAX)")
                    .UseCollation("Arabic_CI_AS");

                entity.Property(e => e.MessageType)
                    .HasMaxLength(50)
                    .HasDefaultValue("text");

                entity.Property(e => e.FilePath)
                    .HasMaxLength(500);

                entity.Property(e => e.FileName)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("sent");

                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // العلاقات
                entity.HasOne(m => m.SenderUser)
                    .WithMany(u => u.MessagesSent)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.ReceiverUser)
                    .WithMany(u => u.MessagesReceived)
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is User &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    user.UpdatedAt = DateTime.Now;
                    if (entry.State == EntityState.Added)
                    {
                        user.CreatedAt = DateTime.Now;
                    }
                }
            }
        }

        // ========== الدوال المساعدة المضافة ==========

        // جلب المستخدمين المتصلين
        public async Task<List<User>> GetOnlineUsersAsync()
        {
            return await Users
                .Where(u => u.IsOnline)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        // جلب الرسائل الحديثة
        public async Task<List<Models.Message>> GetRecentMessagesAsync(
            int count = 100,
            bool isPrivate = false,
            string? currentUser = null,
            string? targetUser = null)
        {
            var query = Messages.AsQueryable();

            if (isPrivate && !string.IsNullOrEmpty(currentUser) &&
                !string.IsNullOrEmpty(targetUser))
            {
                query = query.Where(m => m.IsPrivate &&
                    ((m.Sender == currentUser && m.Receiver == targetUser) ||
                     (m.Sender == targetUser && m.Receiver == currentUser)));
            }
            else if (!isPrivate)
            {
                query = query.Where(m => !m.IsPrivate);
            }

            return await query
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        // جلب عدد الرسائل غير المقروءة
        public async Task<int> GetUnreadCountAsync(string username)
        {
            return await Messages
                .CountAsync(m => m.Receiver == username &&
                    !m.IsRead && m.IsPrivate);
        }

        // ========== الدوال الجديدة ==========

        // جلب صورة الملف الشخصي
        public async Task<string> GetProfileImageAsync(string username)
        {
            try
            {
                var user = await Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user != null && !string.IsNullOrEmpty(user.Avatar))
                {
                    return user.Avatar;
                }

                return "👤"; // قيمة افتراضية
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب صورة الملف الشخصي للمستخدم {username}", ex);
                return "👤";
            }
        }

        // جلب مسار صورة الملف الشخصي
        public async Task<string> GetProfileImagePathAsync(string username)
        {
            try
            {
                string defaultPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "default_profile.png");

                var user = await Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.Avatar) &&
                        File.Exists(user.Avatar))
                    {
                        return user.Avatar;
                    }

                    string userFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "YemenWhatsApp",
                        "Users",
                        username,
                        "profile.jpg"
                    );

                    if (File.Exists(userFolder))
                    {
                        return userFolder;
                    }
                }

                return defaultPath;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب مسار صورة الملف الشخصي", ex);
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default_profile.png");
            }
        }

        // جلب بيانات المستخدم
        public async Task<User> GetUserProfileAsync(string username)
        {
            try
            {
                return await Users
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب بيانات المستخدم {username}", ex);
                return null;
            }
        }

        // تحديث صورة الملف الشخصي
        public async Task<bool> UpdateProfileImageAsync(string username, string avatarOrPath)
        {
            try
            {
                var user = await Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user != null)
                {
                    user.Avatar = avatarOrPath;
                    user.UpdatedAt = DateTime.Now;

                    await SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في تحديث صورة الملف الشخصي للمستخدم {username}", ex);
                return false;
            }
        }

        // جلب معلومات المستخدم
        public async Task<UserInfo> GetUserInfoAsync(string username)
        {
            try
            {
                var user = await Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return null;

                var messageCount = await Messages
                    .CountAsync(m => m.Sender == username || m.Receiver == username);

                var lastMessage = await Messages
                    .Where(m => m.Sender == username || m.Receiver == username)
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync();

                var unreadCount = await GetUnreadCountAsync(username);

                return new UserInfo
                {
                    User = user,
                    MessageCount = messageCount,
                    LastMessageTime = lastMessage?.Timestamp,
                    UnreadCount = unreadCount,
                    IsOnline = user.IsOnline
                };
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب معلومات المستخدم {username}", ex);
                return null;
            }
        }

        // البحث عن المستخدمين
        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                return await Users
                    .Where(u => u.Username.Contains(searchTerm) ||
                               (u.Bio != null && u.Bio.Contains(searchTerm)))
                    .OrderBy(u => u.Username)
                    .Take(50)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في البحث عن المستخدمين: {searchTerm}", ex);
                return new List<User>();
            }
        }

        // جلب سجل الدردشة
        public async Task<List<ChatHistory>> GetChatHistoryAsync(string user1, string user2)
        {
            try
            {
                var messages = await Messages
                    .Where(m => (m.Sender == user1 && m.Receiver == user2) ||
                               (m.Sender == user2 && m.Receiver == user1))
                    .OrderBy(m => m.Timestamp)
                    .Take(200)
                    .ToListAsync();

                return messages.Select(m => new ChatHistory
                {
                    Message = m,
                    IsSentByMe = m.Sender == user1
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب سجل الدردشة بين {user1} و {user2}", ex);
                return new List<ChatHistory>();
            }
        }

        // تحديث حالة قراءة الرسائل
        public async Task<int> MarkMessagesAsReadAsync(string username, string sender)
        {
            try
            {
                var messages = await Messages
                    .Where(m => m.Receiver == username &&
                               m.Sender == sender &&
                               !m.IsRead)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.IsRead = true;
                    message.Status = "read";
                }

                return await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في تحديث حالة قراءة الرسائل", ex);
                return 0;
            }
        }

        // جلب الإحصائيات
        public async Task<ChatStatistics> GetChatStatisticsAsync()
        {
            try
            {
                var stats = new ChatStatistics
                {
                    TotalUsers = await Users.CountAsync(),
                    OnlineUsers = await Users.CountAsync(u => u.IsOnline),
                    TotalMessages = await Messages.CountAsync(),
                    PrivateMessages = await Messages.CountAsync(m => m.IsPrivate),
                    PublicMessages = await Messages.CountAsync(m => !m.IsPrivate),
                    TodayMessages = await Messages.CountAsync(m =>
                        m.Timestamp.Date == DateTime.Today),
                    UnreadMessages = await Messages.CountAsync(m => !m.IsRead),
                    LastActivity = await Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Timestamp)
                        .FirstOrDefaultAsync()
                };

                return stats;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"خطأ في جلب الإحصائيات", ex);
                return new ChatStatistics();
            }
        }
    }

    // ========== فصول المساعدة المضافة ==========

    public class UserInfo
    {
        public User User { get; set; }
        public int MessageCount { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatHistory
    {
        public Models.Message Message { get; set; }
        public bool IsSentByMe { get; set; }
    }

    public class ChatStatistics
    {
        public int TotalUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int TotalMessages { get; set; }
        public int PrivateMessages { get; set; }
        public int PublicMessages { get; set; }
        public int TodayMessages { get; set; }
        public int UnreadMessages { get; set; }
        public DateTime? LastActivity { get; set; }

        public override string ToString()
        {
            return $"المستخدمون: {TotalUsers} | متصلون: {OnlineUsers} | " +
                   $"الرسائل: {TotalMessages} | اليوم: {TodayMessages}";
        }
    }
}