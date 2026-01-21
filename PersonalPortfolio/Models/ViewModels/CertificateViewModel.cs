using System.ComponentModel.DataAnnotations;

namespace PersonalPortfolio.Models.ViewModels
{
    public class CertificateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Certificate name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issuing organization is required")]
        [StringLength(200)]
        [Display(Name = "Issuing Organization")]
        public string IssuingOrganization { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Credential ID")]
        public string? CredentialId { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Credential URL")]
        public string? CredentialUrl { get; set; }

        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }

        [Display(Name = "Expiration Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Does Not Expire")]
        public bool DoesNotExpire { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Certificate Image")]
        public IFormFile? Image { get; set; }

        public string? CurrentImagePath { get; set; }
    }
}
