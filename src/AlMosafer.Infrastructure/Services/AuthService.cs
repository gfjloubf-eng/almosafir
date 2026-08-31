using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AlMosaferDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;

    public AuthService(AlMosaferDbContext dbContext, IPasswordHasherService passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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
