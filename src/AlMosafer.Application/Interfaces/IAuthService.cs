using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Domain.Entities;

namespace AlMosafer.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterTravelerAsync(RegisterTravelerDto dto);
    Task<(bool Success, string Message, User? User)> RegisterDriverAsync(RegisterDriverDto dto);
    Task<(bool Success, string Message, User? User)> ValidateLoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetUserProfileAsync(int userId);
    Task<(bool Success, string Message)> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task SeedDefaultAdminAsync(string adminEmail, string adminPassword);
    Task<(bool Success, string Message)> RequestPasswordResetAsync(string email, string resetUrlTemplate);
    Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
}