using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalPortfolio.Models
{
    public class Education
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "School/Institution is required")]
        [StringLength(200)]
        public string School { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree is required")]
        [StringLength(200)]
        public string Degree { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FieldOfStudy { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [NotMapped]
        public string Institution
        {
            get => School;
            set => School = value;
        }

        [NotMapped]
        public bool IsCurrentlyStudying
        {
            get => EndDate == null;
            set { if (value) EndDate = null; }
        }

        [StringLength(50)]
        public string? Grade { get; set; }
    }
}