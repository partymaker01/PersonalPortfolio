namespace PersonalPortfolio.Models.ViewModels
{
    public class PortfolioViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IEnumerable<Skill> Skills { get; set; } = new List<Skill>();
        public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        public IEnumerable<Education> Educations { get; set; } = new List<Education>();
        public IEnumerable<Experience> Experiences { get; set; } = new List<Experience>();
        public IEnumerable<Certificate> Certificates { get; set; } = new List<Certificate>();
    }
}
