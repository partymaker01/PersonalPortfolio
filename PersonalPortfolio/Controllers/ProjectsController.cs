using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;
using PersonalPortfolio.Models.ViewModels;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool IsValidImageFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return true;
            if (file.Length > MaxFileSize)
                return false;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
            return View(projects);
        }

        public IActionResult Create()
        {
            return View(new ProjectViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (!IsValidImageFile(model.Image))
            {
                ModelState.AddModelError("Image", "Invalid file. Max size: 10MB. Allowed formats: JPG, PNG, GIF, WebP");
            }

            if (ModelState.IsValid)
            {
                var project = new Project
                {
                    Title = model.Title,
                    Description = model.Description,
                    ProjectUrl = model.ProjectUrl,
                    GitHubUrl = model.GitHubUrl,
                    Technologies = model.Technologies,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UserId = _userManager.GetUserId(User)!,
                    CreatedAt = DateTime.UtcNow
                };

                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "projects");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        project.ImagePath = $"/uploads/projects/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Image", $"Error uploading file: {ex.Message}");
                        return View(model);
                    }
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return NotFound();

            var viewModel = new ProjectViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CurrentImagePath = project.ImagePath,
                ProjectUrl = project.ProjectUrl,
                GitHubUrl = project.GitHubUrl,
                Technologies = project.Technologies,
                IsFeatured = project.IsFeatured,
                DisplayOrder = project.DisplayOrder,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!IsValidImageFile(model.Image))
            {
                ModelState.AddModelError("Image", "Invalid file. Max size: 10MB. Allowed formats: JPG, PNG, GIF, WebP");
            }

            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "projects");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        if (!string.IsNullOrEmpty(project.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, project.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        project.ImagePath = $"/uploads/projects/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Image", $"Error uploading file: {ex.Message}");
                        model.CurrentImagePath = project.ImagePath;
                        return View(model);
                    }
                }

                project.Title = model.Title;
                project.Description = model.Description;
                project.ProjectUrl = model.ProjectUrl;
                project.GitHubUrl = model.GitHubUrl;
                project.Technologies = model.Technologies;
                project.IsFeatured = model.IsFeatured;
                project.DisplayOrder = model.DisplayOrder;
                project.StartDate = model.StartDate;
                project.EndDate = model.EndDate;
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            model.CurrentImagePath = project.ImagePath;
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return NotFound();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project != null)
            {
                if (!string.IsNullOrEmpty(project.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, project.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
