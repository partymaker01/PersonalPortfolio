using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SkillsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var skills = await _context.Skills
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return View(skills);
        }

        public IActionResult Create()
        {
            var model = new Skill
            {
                Proficiency = 50,
                DisplayOrder = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            var userId = _userManager.GetUserId(User);

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            Console.WriteLine($"=== SKILL CREATE DEBUG ===");
            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"Skill Name: {skill.Name}");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"  - {error.ErrorMessage}");
                }
                return View(skill);
            }

            skill.UserId = userId!;
            skill.CreatedAt = DateTime.UtcNow;

            Console.WriteLine($"Before Save - Skill ID: {skill.Id}");

            _context.Add(skill);
            await _context.SaveChangesAsync();

            Console.WriteLine($"After Save - Skill ID: {skill.Id}");

            var allSkills = await _context.Skills.Where(s => s.UserId == userId).ToListAsync();
            Console.WriteLine($"Total skills for user {userId}: {allSkills.Count}");
            foreach (var s in allSkills)
            {
                Console.WriteLine($"  - Skill: {s.Name} (ID: {s.Id}, UserID: {s.UserId})");
            }

            TempData["SuccessMessage"] = $"Skill created! Total: {allSkills.Count}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Skill skill)
        {
            if (id != skill.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingSkill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (existingSkill == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingSkill.Name = skill.Name;
                existingSkill.Proficiency = skill.Proficiency;
                existingSkill.Category = skill.Category;
                existingSkill.DisplayOrder = skill.DisplayOrder;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Skill updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(skill);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Skill deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}