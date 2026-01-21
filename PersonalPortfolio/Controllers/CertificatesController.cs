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
    public class CertificatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public CertificatesController(
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
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(certificates);
        }

        public IActionResult Create()
        {
            return View(new CertificateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model)
        {
            if (!IsValidImageFile(model.Image))
            {
                ModelState.AddModelError("Image", "Invalid file. Max size: 10MB. Allowed formats: JPG, PNG, GIF, WebP");
            }

            if (ModelState.IsValid)
            {
                var certificate = new Certificate
                {
                    Name = model.Name,
                    IssuingOrganization = model.IssuingOrganization,
                    CredentialId = model.CredentialId,
                    CredentialUrl = model.CredentialUrl,
                    IssueDate = model.IssueDate,
                    ExpirationDate = model.ExpirationDate,
                    DoesNotExpire = model.DoesNotExpire,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    UserId = _userManager.GetUserId(User)!,
                    CreatedAt = DateTime.UtcNow
                };

                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "certificates");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        certificate.ImagePath = $"/uploads/certificates/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Image", $"Error uploading file: {ex.Message}");
                        return View(model);
                    }
                }

                _context.Add(certificate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Certificate created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (certificate == null)
                return NotFound();

            var viewModel = new CertificateViewModel
            {
                Id = certificate.Id,
                Name = certificate.Name,
                IssuingOrganization = certificate.IssuingOrganization,
                CredentialId = certificate.CredentialId,
                CredentialUrl = certificate.CredentialUrl,
                IssueDate = certificate.IssueDate,
                ExpirationDate = certificate.ExpirationDate,
                DoesNotExpire = certificate.DoesNotExpire,
                Description = certificate.Description,
                DisplayOrder = certificate.DisplayOrder,
                CurrentImagePath = certificate.ImagePath
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CertificateViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!IsValidImageFile(model.Image))
            {
                ModelState.AddModelError("Image", "Invalid file. Max size: 10MB. Allowed formats: JPG, PNG, GIF, WebP");
            }

            var userId = _userManager.GetUserId(User);
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (certificate == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (model.Image != null && model.Image.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "certificates");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        if (!string.IsNullOrEmpty(certificate.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, certificate.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        certificate.ImagePath = $"/uploads/certificates/{uniqueFileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Image", $"Error uploading file: {ex.Message}");
                        model.CurrentImagePath = certificate.ImagePath;
                        return View(model);
                    }
                }

                certificate.Name = model.Name;
                certificate.IssuingOrganization = model.IssuingOrganization;
                certificate.CredentialId = model.CredentialId;
                certificate.CredentialUrl = model.CredentialUrl;
                certificate.IssueDate = model.IssueDate;
                certificate.ExpirationDate = model.ExpirationDate;
                certificate.DoesNotExpire = model.DoesNotExpire;
                certificate.Description = model.Description;
                certificate.DisplayOrder = model.DisplayOrder;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Certificate updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            model.CurrentImagePath = certificate.ImagePath;
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (certificate == null)
                return NotFound();

            return View(certificate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (certificate != null)
            {
                if (!string.IsNullOrEmpty(certificate.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, certificate.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Certificate deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
