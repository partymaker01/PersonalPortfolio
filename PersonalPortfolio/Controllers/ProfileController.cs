using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PersonalPortfolio.Models;
using PersonalPortfolio.Models.ViewModels;

namespace PersonalPortfolio.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        private string ResolveWebRootPath() =>
            !string.IsNullOrEmpty(_webHostEnvironment.WebRootPath)
                ? _webHostEnvironment.WebRootPath
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        private string GetFullPathFromWebRoot(string? relativeOrAbsolutePath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
                return ResolveWebRootPath();

            var trimmed = relativeOrAbsolutePath.TrimStart('/', '\\');
            var candidate = Path.IsPathRooted(trimmed) ? trimmed : Path.Combine(ResolveWebRootPath(), trimmed);
            try { return Path.GetFullPath(candidate); }
            catch { return Path.Combine(ResolveWebRootPath(), trimmed); }
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            string? profileImagePath = user.ProfileImagePath;
            if (!string.IsNullOrEmpty(profileImagePath))
            {
                var fullImagePath = GetFullPathFromWebRoot(profileImagePath);
                if (!System.IO.File.Exists(fullImagePath))
                {
                    _logger.LogWarning("Profile image not found on disk for user {UserId}: {Path}", user.Id, fullImagePath);
                    profileImagePath = null;
                }
            }

            var viewModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                JobTitle = user.JobTitle,
                CurrentProfileImagePath = profileImagePath,
                LinkedInUrl = user.LinkedInUrl,
                GitHubUrl = user.GitHubUrl,
                TwitterUrl = user.TwitterUrl,
                WebsiteUrl = user.WebsiteUrl,
                Location = user.Location,
                Email = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            string? uploadedFilePath = null;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                // Validate file size (5MB max)
                if (model.ProfileImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfileImage", "File size must not exceed 5MB.");
                    model.CurrentProfileImagePath = user.ProfileImagePath;
                    return View(model);
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProfileImage", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                    model.CurrentProfileImagePath = user.ProfileImagePath;
                    return View(model);
                }

                var webRoot = ResolveWebRootPath();
                var uploadsFolder = Path.Combine(webRoot, "uploads", "profiles");

                _logger.LogInformation("Starting file upload process...");
                _logger.LogInformation("WebRoot: {WebRoot}", webRoot);
                _logger.LogInformation("UploadsFolder: {UploadsFolder}", uploadsFolder);
                _logger.LogInformation("File size: {FileSize} bytes", model.ProfileImage.Length);
                _logger.LogInformation("File name: {FileName}", model.ProfileImage.FileName);

                try
                {
                    // Ensure directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Verify directory was created
                    if (!Directory.Exists(uploadsFolder))
                    {
                        throw new IOException($"Failed to create directory: {uploadsFolder}");
                    }

                    // Generate unique filename
                    var originalName = Path.GetFileName(model.ProfileImage.FileName) ?? "upload";
                    var sanitizedName = string.Join("_", originalName.Split(Path.GetInvalidFileNameChars()));
                    var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    _logger.LogInformation("Saving file to: {FilePath}", filePath);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }

                    // Verify file was saved
                    if (!System.IO.File.Exists(filePath))
                    {
                        throw new IOException($"File was not saved successfully: {filePath}");
                    }

                    var savedFileSize = new FileInfo(filePath).Length;
                    _logger.LogInformation("File saved successfully. Size: {SavedSize} bytes", savedFileSize);

                    // Store the uploaded file path for later use
                    uploadedFilePath = filePath;

                    // Update user's profile image path
                    user.ProfileImagePath = $"/uploads/profiles/{uniqueFileName}";
                }
                catch (UnauthorizedAccessException uex)
                {
                    _logger.LogError(uex, "Permission denied. WebRoot={WebRoot}, UploadsFolder={UploadsFolder}",
                        webRoot, uploadsFolder);
                    ModelState.AddModelError("ProfileImage", "Cannot save image: permission denied on server.");
                    model.CurrentProfileImagePath = user.ProfileImagePath;
                    return View(model);
                }
                catch (IOException ioex)
                {
                    _logger.LogError(ioex,
                        "I/O Error - Message: {Message}, HResult: {HResult}, WebRoot={WebRoot}, UploadsFolder={UploadsFolder}",
                        ioex.Message, ioex.HResult, webRoot, uploadsFolder);
                    ModelState.AddModelError("ProfileImage",
                        $"Cannot save image due to an I/O error: {ioex.Message}");
                    model.CurrentProfileImagePath = user.ProfileImagePath;
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error. Type: {ExceptionType}, WebRoot={WebRoot}",
                        ex.GetType().Name, webRoot);
                    ModelState.AddModelError("ProfileImage",
                        $"Unexpected error while uploading image: {ex.Message}");
                    model.CurrentProfileImagePath = user.ProfileImagePath;
                    return View(model);
                }
            }

            // Update user profile fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            user.JobTitle = model.JobTitle;
            user.LinkedInUrl = model.LinkedInUrl;
            user.GitHubUrl = model.GitHubUrl;
            user.TwitterUrl = model.TwitterUrl;
            user.WebsiteUrl = model.WebsiteUrl;
            user.Location = model.Location;
            user.UpdatedAt = DateTime.UtcNow;

            // Save all changes to database
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Delete old image AFTER successful database update
                if (!string.IsNullOrEmpty(uploadedFilePath))
                {
                    // Find and delete old image (if it exists and is different from new one)
                    var oldImagePath = model.CurrentProfileImagePath;
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        var oldFullPath = GetFullPathFromWebRoot(oldImagePath);
                        try
                        {
                            if (System.IO.File.Exists(oldFullPath) && oldFullPath != uploadedFilePath)
                            {
                                _logger.LogInformation("Deleting old profile image at {OldImagePath}", oldFullPath);
                                System.IO.File.Delete(oldFullPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old profile image: {OldImagePath}", oldFullPath);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(uploadedFilePath))
            {
                try
                {
                    if (System.IO.File.Exists(uploadedFilePath))
                    {
                        System.IO.File.Delete(uploadedFilePath);
                        _logger.LogInformation("Rolled back uploaded file: {FilePath}", uploadedFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rollback uploaded file: {FilePath}", uploadedFilePath);
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.CurrentProfileImagePath = model.CurrentProfileImagePath;
            return View(model);
        }
    }
}