using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalPortfolio.Data;
using PersonalPortfolio.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});


builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    options.ValueLengthLimit = 10 * 1024 * 1024;
    options.MultipartHeadersLengthLimit = 10 * 1024 * 1024;
});

builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "portfolio",
    pattern: "portfolio/{username}",
    defaults: new { controller = "Home", action = "Portfolio" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


var logger = app.Services.GetRequiredService<ILogger<Program>>();
var webRootPath = app.Environment.WebRootPath;

if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    logger.LogWarning("WebRootPath was null, using fallback: {WebRootPath}", webRootPath);
}
else
{
    logger.LogInformation("WebRootPath resolved to: {WebRootPath}", webRootPath);
}

var uploadPaths = new[]
{
    Path.Combine(webRootPath, "uploads", "profiles"),
    Path.Combine(webRootPath, "uploads", "projects"),
    Path.Combine(webRootPath, "uploads", "certificates")
};

foreach (var path in uploadPaths)
{
    try
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            logger.LogInformation("Created upload directory: {Path}", path);
        }
        else
        {
            logger.LogInformation("Upload directory already exists: {Path}", path);
        }


        var testFile = Path.Combine(path, $"test_{Guid.NewGuid():N}.tmp");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);
        logger.LogInformation("Write permissions verified for: {Path}", path);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create or verify upload directory: {Path}", path);
    }
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        var dbLogger = services.GetRequiredService<ILogger<Program>>();
        dbLogger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();