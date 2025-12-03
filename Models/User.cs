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

        [MaxLength(20)]
        public string Avatar { get; set; } = "👤";

        [MaxLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Message>? MessagesSent { get; set; }
        public virtual ICollection<Message>? MessagesReceived { get; set; }
        public string Name { get; internal set; }
        public string Password { get; internal set; }
        public string Email { get; internal set; }

        public override string ToString()
        {
            return $"{Username} - {(IsOnline ? "🟢 متصل" : "⚫ غير متصل")}";
        }
    }
}