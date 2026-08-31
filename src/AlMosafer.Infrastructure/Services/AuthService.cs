using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AlMosaferDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;

    private readonly IEmailService? _emailService;

    public AuthService(AlMosaferDbContext dbContext, IPasswordHasherService passwordHasher, IEmailService? emailService = null)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    private sealed class PasswordResetPrefs
    {
        [JsonPropertyName("PasswordReset")]
        public PasswordResetEntry? PasswordReset { get; set; }
    }

    private sealed class PasswordResetEntry
    {
        [JsonPropertyName("Token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("ExpiresUtc")]
        public long ExpiresUtc { get; set; }
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email, string resetUrlTemplate)
    {
        // حماية من استعلام الوجود: نرد بنفس الرسالة سواء وُجد البريد أم لا
        const string genericMessage = "إن كان هذا البريد مسجلاً لدينا فستصله تعليمات إعادة التعيين خلال دقائق.";

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
        if (user == null)
        {
            return (true, genericMessage);
        }

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();

        // تخزين الرمز في PreferencesJson (بلا هجرة مخطط) — لقطة واحدة مستقلة حصراً للاستعادة
        Dictionary<string, JsonElement>? prefs = null;
        if (!string.IsNullOrWhiteSpace(user.PreferencesJson))
        {
            try { prefs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(user.PreferencesJson, JsonOpts); } catch (JsonException) { prefs = null; }
        }
        var dict = prefs?.ToDictionary(k => k.Key, v => (object?)v.Value) ?? new Dictionary<string, object?>();
        dict["PasswordReset"] = new Dictionary<string, object> { ["Token"] = token, ["ExpiresUtc"] = expiry };
        user.PreferencesJson = JsonSerializer.Serialize(dict, JsonOpts);
        await _dbContext.SaveChangesAsync();

        var resetUrl = resetUrlTemplate.Replace("__TOKEN__", token);
        var html = $"<div dir="rtl" style="font-family:Tahoma">" +
                   $"<h3>إعادة تعيين كلمة المرور — منصة المسافر</h3>" +
                   $"<p>طلبت استعادة كلمة مرور حسابك. اضغط الرابط خلال 30 دقيقة:</p>" +
                   $"<p><a href="{resetUrl}">{resetUrl}</a></p>" +
                   $"<p style="color:#888">إن لم تطلب ذلك تجاهل هذه الرسالة وستبقى كلمة مرورك كما هي.</p></div>";

        if (_emailService != null)
        {
            await _emailService.SendAsync(user.Email, "إعادة تعيين كلمة المرور — منصة المسافر", html);
        }

        return (true, genericMessage);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length < 32)
        {
            return (false, "رابط إعادة التعيين غير صالح أو منتهي. اطلب رابطاً جديداً.");
        }
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return (false, "كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل.");
        }

        var candidates = await _dbContext.Users
            .Where(u => u.PreferencesJson != null && u.PreferencesJson.Contains(""PasswordReset""))
            .ToListAsync();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        User? matched = null;
        foreach (var u in candidates)
        {
            try
            {
                var pr = JsonSerializer.Deserialize<PasswordResetPrefs>(u.PreferencesJson!, JsonOpts)?.PasswordReset;
                if (pr != null && pr.Token == token && pr.ExpiresUtc >= now)
                {
                    matched = u;
                    break;
                }
            }
            catch (JsonException) { /* تجاهل سجلات تالفة بأمان */ }
        }

        if (matched == null)
        {
            return (false, "رابط إعادة التعيين غير صالح أو منتهي. اطلب رابطاً جديداً.");
        }

        matched.PasswordHash = _passwordHasher.HashPassword(matched, newPassword);

        // رمز استخدام واحد: يُحذف فور النجاح
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(matched.PreferencesJson!, JsonOpts) ?? new Dictionary<string, object>();
            dict.Remove("PasswordReset");
            matched.PreferencesJson = dict.Count > 0 ? JsonSerializer.Serialize(dict, JsonOpts) : null;
        }
        catch (JsonException)
        {
            matched.PreferencesJson = null;
        }

        await _dbContext.SaveChangesAsync();
        return (true, "عُيّنت كلمة المرور الجديدة بنجاح — سجّل الدخول بها الآن.");
    }

    public async Task<(bool Success, string Message, User? User)> RegisterTravelerAsync(RegisterTravelerDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            return (false, "البريد الإلكتروني مستخدم بالفعل في النظام.", null);
        }

        var user = new User
        {
            Name = dto.FullName.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone?.Trim(),
            City = dto.City?.Trim(),
            Role = UserRole.Traveler,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return (true, "تم إنشاء حساب المسافر بنجاح!", user);
    }

    public async Task<(bool Success, string Message, User? User)> RegisterDriverAsync(RegisterDriverDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            return (false, "البريد الإلكتروني مستخدم بالفعل في النظام.", null);
        }

        var user = new User
        {
            Name = dto.FullName.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone.Trim(),
            City = dto.City?.Trim(),
            PlateNumber = dto.PlateNumber.Trim(),
            VehicleModel = dto.VehicleModel.Trim(),
            VehicleYear = dto.VehicleYear,
            Role = UserRole.Driver,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return (true, "تم إنشاء حساب السائق بنجاح!", user);
    }

    public async Task<(bool Success, string Message, User? User)> ValidateLoginAsync(LoginDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة.", null);
        }

        var isValid = _passwordHasher.VerifyPassword(user, user.PasswordHash, dto.Password);
        if (!isValid)
        {
            return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة.", null);
        }

        return (true, "تم تسجيل الدخول بنجاح!", user);
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Photo = user.Photo,
            PlateNumber = user.PlateNumber,
            Rating = user.Rating,
            VehicleModel = user.VehicleModel,
            VehicleYear = user.VehicleYear,
            City = user.City,
            TotalTrips = user.TotalTrips,
            TotalEarnings = user.TotalEarnings,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<(bool Success, string Message)> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود.");
        }

        // Strictly update allowed user fields only. Email, Role, Password, IDOR fields remain untouched!
        user.Name = dto.Name.Trim();
        user.Phone = dto.Phone?.Trim();
        user.City = dto.City?.Trim();

        if (user.Role == UserRole.Driver)
        {
            if (!string.IsNullOrWhiteSpace(dto.VehicleModel))
                user.VehicleModel = dto.VehicleModel.Trim();

            if (!string.IsNullOrWhiteSpace(dto.PlateNumber))
                user.PlateNumber = dto.PlateNumber.Trim();

            if (dto.VehicleYear.HasValue)
                user.VehicleYear = dto.VehicleYear.Value;
        }

        await _dbContext.SaveChangesAsync();
        return (true, "تم تحديث بيانات الملف الشخصي بنجاح.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود.");
        }

        var isValid = _passwordHasher.VerifyPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (!isValid)
        {
            return (false, "كلمة المرور الحالية غير صحيحة.");
        }

        if (dto.CurrentPassword == dto.NewPassword)
        {
            return (false, "كلمة المرور الجديدة يجب أن تكون مختلفة عن كلمة المرور الحالية.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        await _dbContext.SaveChangesAsync();

        return (true, "تم تغيير كلمة المرور بنجاح.");
    }

    public async Task SeedDefaultAdminAsync(string adminEmail, string adminPassword)
    {
        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();
        var adminExists = await _dbContext.Users.AnyAsync(u => u.Role == UserRole.Admin || u.Email.ToLower() == normalizedEmail);

        if (!adminExists)
        {
            var adminUser = new User
            {
                Name = "مدير النظام",
                Email = normalizedEmail,
                Phone = "770000000",
                Role = UserRole.Admin,
                City = "تعز",
                CreatedAt = DateTime.UtcNow
            };

            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, adminPassword);

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync();
        }
    }
}
