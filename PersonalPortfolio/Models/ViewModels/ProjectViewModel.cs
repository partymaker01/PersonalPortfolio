using System.ComponentModel.DataAnnotations;

namespace PersonalPortfolio.Models.ViewModels
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Project Image")]
        public IFormFile? Image { get; set; }

        public string? CurrentImagePath { get; set; }

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
    }
}
