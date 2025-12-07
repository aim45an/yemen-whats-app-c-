using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemenWhatsApp.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sender { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Receiver { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MessageType { get; set; } = "text";

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(500)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        public bool IsPrivate { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "sent";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign keys
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual User? SenderUser { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User? ReceiverUser { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm}] {Sender}: {Content}";
        }
    }
}