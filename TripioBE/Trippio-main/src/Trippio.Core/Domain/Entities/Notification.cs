using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trippio.Core.Domain.Identity;

namespace Trippio.Core.Domain.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}