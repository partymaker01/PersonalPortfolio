using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PersonalPortfolio.Models.ViewModels
{
    public class ProfileViewModel
    {
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        public string? CurrentProfileImagePath { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "LinkedIn URL")]
        public string? LinkedInUrl { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "GitHub URL")]
        public string? GitHubUrl { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Twitter URL")]
        public string? TwitterUrl { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
