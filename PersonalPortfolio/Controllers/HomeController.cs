using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models.ViewModels;
using System.Diagnostics;
using PersonalPortfolio.Models;

namespace PersonalPortfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all users with their portfolios for the home page
            var users = await _context.Users
                .Include(u => u.Skills)
                .Include(u => u.Projects.Where(p => p.IsFeatured))
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Portfolio(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Skills.OrderBy(s => s.DisplayOrder))
                .Include(u => u.Projects.OrderBy(p => p.DisplayOrder))
                .Include(u => u.Educations.OrderBy(e => e.DisplayOrder))
                .Include(u => u.Experiences.OrderBy(e => e.DisplayOrder))
                .Include(u => u.Certificates.OrderBy(c => c.DisplayOrder))
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new PortfolioViewModel
            {
                User = user,
                Skills = user.Skills,
                Projects = user.Projects,
                Educations = user.Educations,
                Experiences = user.Experiences,
                Certificates = user.Certificates
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
