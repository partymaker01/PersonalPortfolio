using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalPortfolio.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Project URL")]
        public string? ProjectUrl { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "GitHub URL")]
        public string? GitHubUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Technologies Used")]
        public string? Technologies { get; set; }

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}