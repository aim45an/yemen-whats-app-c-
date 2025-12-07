using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemenWhatsApp.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ConnectionId { get; set; }

        public bool IsOnline { get; set; } = true;

        public DateTime LastSeen { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string Status { get; set; } = "متصل";

        [MaxLength(50)]
        public string Color { get; set; } = "#0078D7";

        [MaxLength(500)]
        public string Avatar { get; set; } = "👤";

        [MaxLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // الخصائص الإضافية (اختيارية)
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        public int LoginCount { get; set; } = 0;

        // الإعدادات الشخصية
        public bool NotificationsEnabled { get; set; } = true;

        public bool SoundEnabled { get; set; } = true;

        [MaxLength(50)]
        public string Theme { get; set; } = "light";

        // الإحصائيات
        public int TotalMessagesSent { get; set; } = 0;

        public int TotalMessagesReceived { get; set; } = 0;

        public int TotalFilesSent { get; set; } = 0;

        // العلاقات
        public virtual ICollection<Message>? MessagesSent { get; set; }

        public virtual ICollection<Message>? MessagesReceived { get; set; }

        // الخاصية المضافة لتجنب الخطأ
        [NotMapped] // لا تخزن في قاعدة البيانات
        public string Password { get; set; } = string.Empty;
        public bool IsVerified { get; internal set; }

        public override string ToString()
        {
            return $"{Username} - {(IsOnline ? "🟢 متصل" : "⚫ غير متصل")}";
        }

        // دالة تحديث آخر ظهور
        public void UpdateLastSeen()
        {
            LastSeen = DateTime.Now;
            if (IsOnline)
            {
                Status = "متصل";
            }
            else
            {
                Status = "آخر ظهور " + LastSeen.ToString("HH:mm");
            }
        }

        // دالة تسجيل الدخول
        public void Login()
        {
            IsOnline = true;
            LastLogin = DateTime.Now;
            LoginCount++;
            Status = "متصل";
            UpdateLastSeen();
        }

        // دالة تسجيل الخروج
        public void Logout()
        {
            IsOnline = false;
            Status = "غير متصل";
            UpdateLastSeen();
        }

        // دالة تحديث الحالة
        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.Now;
        }

        // دالة تحديث الصورة
        public void UpdateAvatar(string newAvatar)
        {
            Avatar = newAvatar;
            UpdatedAt = DateTime.Now;
        }

        // دالة تحديث اللون
        public void UpdateColor(string newColor)
        {
            Color = newColor;
            UpdatedAt = DateTime.Now;
        }

        // دالة جلب معلومات المستخدم
        public UserInfo GetUserInfo()
        {
            return new UserInfo
            {
                User = this,
                IsOnline = this.IsOnline,
                LastActivity = this.LastSeen
            };
        }
    }

    // فئة معلومات المستخدم المبسطة
    public class UserInfo
    {
        public User User { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastActivity { get; set; }
        public string DisplayName => User.Username;
        public string Status => User.Status;
        public string Avatar => User.Avatar;
        public string Color => User.Color;
    }
}