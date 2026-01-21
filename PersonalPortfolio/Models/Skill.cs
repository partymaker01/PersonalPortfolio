using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalPortfolio.Models
{
    public class Skill
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Skill name is required")]
        [StringLength(100)]
        [Display(Name = "Skill Name")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Proficiency must be between 0 and 100")]
        [Display(Name = "Proficiency (%)")]
        public int Proficiency { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}