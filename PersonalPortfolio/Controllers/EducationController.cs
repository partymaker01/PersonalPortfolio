using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class EducationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EducationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var educations = await _context.Educations
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            return View(educations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Education education)
        {
            if (ModelState.IsValid)
            {
                education.UserId = _userManager.GetUserId(User)!;
                education.CreatedAt = DateTime.UtcNow;

                _context.Add(education);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Education record created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(education);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (education == null)
            {
                return NotFound();
            }

            return View(education);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Education education)
        {
            if (id != education.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingEducation = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (existingEducation == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update with actual properties from Education model
                existingEducation.School = education.School;
                existingEducation.Degree = education.Degree;
                existingEducation.FieldOfStudy = education.FieldOfStudy;
                existingEducation.Description = education.Description;
                existingEducation.StartDate = education.StartDate;
                existingEducation.EndDate = education.EndDate;
                existingEducation.DisplayOrder = education.DisplayOrder;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Education record updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(education);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (education == null)
            {
                return NotFound();
            }

            return View(education);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (education != null)
            {
                _context.Educations.Remove(education);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Education record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}