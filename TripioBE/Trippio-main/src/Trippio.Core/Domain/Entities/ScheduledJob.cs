using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trippio.Core.Domain.Entities
{
    [Table("ScheduledJobs")]
    public class ScheduledJob
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string JobType { get; set; }

        [Required]
        public required string Payload { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}