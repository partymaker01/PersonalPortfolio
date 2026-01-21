using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class ExperienceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExperienceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var experiences = await _context.Experiences
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            return View(experiences);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Experience experience)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // Debug logging
                Console.WriteLine($"=== CREATING EXPERIENCE ===");
                Console.WriteLine($"User ID: {userId}");
                Console.WriteLine($"Job Title: {experience.JobTitle}");
                Console.WriteLine($"Company: {experience.Company}");

                experience.UserId = userId!;
                experience.CreatedAt = DateTime.UtcNow;

                _context.Add(experience);
                await _context.SaveChangesAsync();

                // Verify it was saved
                var count = await _context.Experiences.CountAsync(e => e.UserId == userId);
                Console.WriteLine($"Total experiences after save: {count}");

                TempData["SuccessMessage"] = "Experience record created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Log validation errors
            Console.WriteLine("=== VALIDATION ERRORS ===");
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }

            return View(experience);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var experience = await _context.Experiences
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Experience experience)
        {
            if (id != experience.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingExperience = await _context.Experiences
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (existingExperience == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingExperience.JobTitle = experience.JobTitle;
                existingExperience.Company = experience.Company;
                existingExperience.Location = experience.Location;
                existingExperience.Description = experience.Description;
                existingExperience.StartDate = experience.StartDate;
                existingExperience.EndDate = experience.EndDate;
                existingExperience.IsCurrent = experience.IsCurrent;
                existingExperience.DisplayOrder = experience.DisplayOrder;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Experience record updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(experience);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var experience = await _context.Experiences
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var experience = await _context.Experiences
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (experience != null)
            {
                _context.Experiences.Remove(experience);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Experience record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}