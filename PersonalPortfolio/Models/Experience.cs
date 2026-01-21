using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalPortfolio.Models
{
    public class Experience
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company is required")]
        [StringLength(200)]
        public string Company { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [NotMapped]
        public bool IsCurrentlyWorking
        {
            get => IsCurrent;
            set => IsCurrent = value;
        }
    }
}