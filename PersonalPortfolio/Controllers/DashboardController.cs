using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;
using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            Console.WriteLine($"=== DASHBOARD DEBUG ===");
            Console.WriteLine($"Current User ID: {userId}");
            Console.WriteLine($"User Email: {user?.Email}");

            var skillsCount = await _context.Skills.CountAsync(s => s.UserId == userId);
            var allSkillsInDb = await _context.Skills.ToListAsync();

            Console.WriteLine($"Skills count for user {userId}: {skillsCount}");
            Console.WriteLine($"Total skills in database: {allSkillsInDb.Count}");
            Console.WriteLine("All skills in database:");
            foreach (var s in allSkillsInDb)
            {
                Console.WriteLine($"  - {s.Name} (UserID: {s.UserId}, Match: {s.UserId == userId})");
            }

            var projectsCount = await _context.Projects.CountAsync(p => p.UserId == userId);
            var educationsCount = await _context.Educations.CountAsync(e => e.UserId == userId);
            var experiencesCount = await _context.Experiences.CountAsync(e => e.UserId == userId);
            var certificatesCount = await _context.Certificates.CountAsync(c => c.UserId == userId);
            var unreadMessagesCount = await _context.Contacts.CountAsync(c => c.UserId == userId && !c.IsRead);

            ViewBag.SkillsCount = skillsCount;
            ViewBag.ProjectsCount = projectsCount;
            ViewBag.EducationsCount = educationsCount;
            ViewBag.ExperiencesCount = experiencesCount;
            ViewBag.CertificatesCount = certificatesCount;
            ViewBag.UnreadMessagesCount = unreadMessagesCount;

            return View(user);
        }
    }
}
