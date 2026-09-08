using System.Security.Claims;
using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlMosafer.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;
    private readonly IWebHostEnvironment _env;

    public AccountController(IAuthService authService, ILogger<AccountController> logger, IWebHostEnvironment env)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
    }

    [HttpGet]
    public IActionResult RegisterTraveler()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboard();
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> RegisterTraveler(RegisterTravelerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.RegisterTravelerAsync(dto);
        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        await SignInUserAsync(result.User, isPersistent: false);
        TempData["SuccessMessage"] = "مرحباً بك! تم إنشاء حسابك كمسافر بنجاح.";

        return RedirectToDashboard();
    }

    [HttpGet]
    public IActionResult RegisterDriver()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboard();
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> RegisterDriver(RegisterDriverDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.RegisterDriverAsync(dto);
        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        await SignInUserAsync(result.User, isPersistent: false);
        TempData["SuccessMessage"] = "أهلاً بك كـ سائق شريك في المسافر!";

        return RedirectToDashboard();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboard();
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]    [ValidateAntiForgeryToken]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "أدخل بريدك الإلكتروني.");
            return View();
        }

        var urlTemplate = Url.Action(nameof(ResetPassword), "Account", new { token = "__TOKEN__" }, Request.Scheme) ?? string.Empty;
        var result = await _authService.RequestPasswordResetAsync(email, urlTemplate);
        TempData["InfoMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Login));
        }
        ViewBag.Token = token;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "كلمتا المرور غير متطابقتين.";
            ViewBag.Token = token;
            return View();
        }

        var result = await _authService.ResetPasswordAsync(token, newPassword);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            ViewBag.Token = token;
            return View();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.ValidateLoginAsync(dto);
        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        await SignInUserAsync(result.User, dto.RememberMe);
        TempData["SuccessMessage"] = $"مرحباً بعودتك، {result.User.Name}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToDashboard();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["InfoMessage"] = "تم تسجيل الخروج بنجاح.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        var dto = new UpdateUserProfileDto
        {
            Name = profile.Name,
            Phone = profile.Phone,
            City = profile.City,
            VehicleModel = profile.VehicleModel,
            PlateNumber = profile.PlateNumber,
            VehicleYear = profile.VehicleYear
        };

        ViewBag.CurrentPhoto = profile.Photo;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> EditProfile(UpdateUserProfileDto dto, IFormFile? photoFile)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = GetCurrentUserId();

        // رفع الصورة الشخصية (اختياري): امتداد مسموح + حجم أقصى 2MB
        if (photoFile != null && photoFile.Length > 0)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "صيغة الصورة غير مدعومة. استخدم JPG أو PNG أو GIF أو WebP.");
                return View(dto);
            }
            if (photoFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "حجم الصورة يتجاوز الحد الأقصى (2 ميجابايت).");
                return View(dto);
            }

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"user_{userId}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await photoFile.CopyToAsync(stream);
            }

            var photoResult = await _authService.UpdateUserPhotoAsync(userId, $"/uploads/profiles/{fileName}");
            if (!photoResult.Success)
            {
                ModelState.AddModelError(string.Empty, photoResult.Message);
                return View(dto);
            }
        }

        var result = await _authService.UpdateUserProfileAsync(userId, dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    [EnableRateLimiting("StrictLimiter")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = GetCurrentUserId();
        var result = await _authService.ChangePasswordAsync(userId, dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(Domain.Entities.User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToDashboard()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        else if (User.IsInRole("Driver"))
        {
            return RedirectToAction("Dashboard", "Driver");
        }
        else
        {
            return RedirectToAction("Dashboard", "Traveler");
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
