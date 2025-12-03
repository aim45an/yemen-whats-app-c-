using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YemenWhatsApp.Models;

namespace YemenWhatsApp.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public ChatDbContext()
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Models.Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // سلسلة الاتصال الافتراضية
                string connectionString = @"Server=DESKTOP-2U7RVGF;
                                      Database=YemenChatDB;Trusted_Connection=True;TrustServerCertificate=True;";


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
                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                    Microsoft.Extensions.Logging.LogLevel.Information);
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
                      .HasMaxLength(20)
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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

        // طرق مساعدة
        public async Task<List<User>> GetOnlineUsersAsync()
        {
            return await Users
                .Where(u => u.IsOnline)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<List<Models.Message>> GetRecentMessagesAsync(int count = 100, bool isPrivate = false, string? currentUser = null, string? targetUser = null)
        {
            var query = Messages.AsQueryable();

            if (isPrivate && !string.IsNullOrEmpty(currentUser) && !string.IsNullOrEmpty(targetUser))
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

        public async Task<int> GetUnreadCountAsync(string username)
        {
            return await Messages
                .CountAsync(m => m.Receiver == username && !m.IsRead && m.IsPrivate);
        }
    }
}