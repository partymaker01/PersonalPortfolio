//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PersonalPortfolio.Data;
//using PersonalPortfolio.Models;

//namespace PersonalPortfolio.Controllers
//{
//    [Authorize]
//    public class DiagnosticController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public DiagnosticController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var userId = _userManager.GetUserId(User);
//            var user = await _userManager.GetUserAsync(User);

//            var diagnosticInfo = new
//            {
//                CurrentUserId = userId,
//                UserEmail = user?.Email,
//                UserName = User.Identity?.Name,

//                // Get all data with UserIds
//                AllEducations = await _context.Educations
//                    .Select(e => new { e.Id, e.School, e.Degree, e.UserId })
//                    .ToListAsync(),

//                AllExperiences = await _context.Experiences
//                    .Select(e => new { e.Id, e.JobTitle, e.Company, e.UserId })
//                    .ToListAsync(),

//                AllContacts = await _context.Contacts
//                    .Select(c => new { c.Id, c.Name, c.Email, c.UserId, c.IsRead })
//                    .ToListAsync(),

//                // Count for current user
//                MyEducationCount = await _context.Educations.CountAsync(e => e.UserId == userId),
//                MyExperienceCount = await _context.Experiences.CountAsync(e => e.UserId == userId),
//                MyContactCount = await _context.Contacts.CountAsync(c => c.UserId == userId),
//                MyUnreadContactCount = await _context.Contacts.CountAsync(c => c.UserId == userId && !c.IsRead),

//                // Check if UserId is null or empty
//                EducationsWithNullUserId = await _context.Educations.CountAsync(e => string.IsNullOrEmpty(e.UserId)),
//                ExperiencesWithNullUserId = await _context.Experiences.CountAsync(e => string.IsNullOrEmpty(e.UserId)),
//                ContactsWithNullUserId = await _context.Contacts.CountAsync(c => string.IsNullOrEmpty(c.UserId))
//            };

//            return Json(diagnosticInfo);
//        }
//    }
//}