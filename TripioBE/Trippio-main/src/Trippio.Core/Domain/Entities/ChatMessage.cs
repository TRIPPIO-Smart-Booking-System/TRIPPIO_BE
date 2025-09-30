using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trippio.Core.Domain.Identity;

namespace Trippio.Core.Domain.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid SenderUserId { get; set; }

        [Required]
        public Guid ReceiverUserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Content { get; set; }

        [Required]
        public DateTime SentTime { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!;

        [ForeignKey("SenderUserId")]
        public virtual AppUser SenderUser { get; set; } = null!;

        [ForeignKey("ReceiverUserId")]
        public virtual AppUser ReceiverUser { get; set; } = null!;
    }
}