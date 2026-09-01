using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class AuthServiceTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task RegisterTraveler_ValidData_CreatesTravelerWithHashedPassword()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var dto = new RegisterTravelerDto
        {
            FullName = "صالح المسافر",
            Email = "saleh@traveler.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Phone = "771111111",
            City = "تعز"
        };

        var result = await authService.RegisterTravelerAsync(dto);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("صالح المسافر", result.User.Name);
        Assert.Equal(UserRole.Traveler, result.User.Role);
        Assert.NotEqual("Password123!", result.User.PasswordHash);
        Assert.True(passwordHasher.VerifyPassword(result.User, result.User.PasswordHash, "Password123!"));
    }

    [Fact]
    public async Task RegisterDriver_ValidData_CreatesDriverWithVehicleInfo()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var dto = new RegisterDriverDto
        {
            FullName = "عمر السائق",
            Email = "omar@driver.com",
            Password = "DriverPassword123!",
            ConfirmPassword = "DriverPassword123!",
            Phone = "772222222",
            PlateNumber = "777-اليمن",
            VehicleModel = "Toyota Hilux",
            VehicleYear = 2022,
            City = "عدن"
        };

        var result = await authService.RegisterDriverAsync(dto);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(UserRole.Driver, result.User.Role);
        Assert.Equal("777-اليمن", result.User.PlateNumber);
        Assert.Equal("Toyota Hilux", result.User.VehicleModel);
    }

    [Fact]
    public async Task ValidateLogin_CorrectCredentials_ReturnsSuccess()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "خالد",
            Email = "khaled@test.com",
            Password = "MySecretPassword1",
            ConfirmPassword = "MySecretPassword1"
        });

        var loginResult = await authService.ValidateLoginAsync(new LoginDto
        {
            Email = "khaled@test.com",
            Password = "MySecretPassword1"
        });

        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.User);
        Assert.Equal("خالد", loginResult.User.Name);
    }

    [Fact]
    public async Task ValidateLogin_WrongPassword_ReturnsFailure()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "خالد",
            Email = "khaled@test.com",
            Password = "MySecretPassword1",
            ConfirmPassword = "MySecretPassword1"
        });

        var loginResult = await authService.ValidateLoginAsync(new LoginDto
        {
            Email = "khaled@test.com",
            Password = "WrongPassword999"
        });

        Assert.False(loginResult.Success);
        Assert.Null(loginResult.User);
    }

    [Fact]
    public async Task GetProfile_ReturnsCurrentUserProfileWithoutSensitiveData()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var reg = await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "مروان",
            Email = "marwan@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            City = "تعز"
        });

        var profile = await authService.GetUserProfileAsync(reg.User!.Id);

        Assert.NotNull(profile);
        Assert.Equal("مروان", profile.Name);
        Assert.Equal("marwan@test.com", profile.Email);
        Assert.Equal(UserRole.Traveler, profile.Role);
    }

    [Fact]
    public async Task UpdateUserProfile_UpdatesAllowedFields_PreservesRoleAndEmail()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var reg = await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "سامي القديم",
            Email = "sami@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Phone = "770000000",
            City = "إب"
        });

        var updateDto = new UpdateUserProfileDto
        {
            Name = "سامي الجديد",
            Phone = "779999999",
            City = "صنعاء"
        };

        var updateResult = await authService.UpdateUserProfileAsync(reg.User!.Id, updateDto);
        Assert.True(updateResult.Success);

        var updatedUser = await dbContext.Users.FindAsync(reg.User.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("سامي الجديد", updatedUser.Name);
        Assert.Equal("779999999", updatedUser.Phone);
        Assert.Equal("صنعاء", updatedUser.City);
        // Security Check: Role and Email remain completely intact!
        Assert.Equal("sami@test.com", updatedUser.Email);
        Assert.Equal(UserRole.Traveler, updatedUser.Role);
    }

    [Fact]
    public async Task ChangePassword_InvalidCurrentPassword_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var reg = await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "ياسر",
            Email = "yasser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongOldPassword",
            NewPassword = "NewSecretPassword123!",
            ConfirmNewPassword = "NewSecretPassword123!"
        };

        var result = await authService.ChangePasswordAsync(reg.User!.Id, changeDto);
        Assert.False(result.Success);
        Assert.Equal("كلمة المرور الحالية غير صحيحة.", result.Message);
    }

    [Fact]
    public async Task ChangePassword_SameNewPassword_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var reg = await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "طارق",
            Email = "tariq@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "Password123!",
            NewPassword = "Password123!",
            ConfirmNewPassword = "Password123!"
        };

        var result = await authService.ChangePasswordAsync(reg.User!.Id, changeDto);
        Assert.False(result.Success);
        Assert.Equal("كلمة المرور الجديدة يجب أن تكون مختلفة عن كلمة المرور الحالية.", result.Message);
    }

    [Fact]
    public async Task ChangePassword_ValidCredentials_SucceedsAndAllowsNewLogin()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var passwordHasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, passwordHasher);

        var reg = await authService.RegisterTravelerAsync(new RegisterTravelerDto
        {
            FullName = "وسيم",
            Email = "waseem@test.com",
            Password = "OldPassword123!",
            ConfirmPassword = "OldPassword123!"
        });

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "BrandNewPassword123!",
            ConfirmNewPassword = "BrandNewPassword123!"
        };

        var changeResult = await authService.ChangePasswordAsync(reg.User!.Id, changeDto);
        Assert.True(changeResult.Success);

        // Verify login with new password
        var loginResult = await authService.ValidateLoginAsync(new LoginDto
        {
            Email = "waseem@test.com",
            Password = "BrandNewPassword123!"
        });
        Assert.True(loginResult.Success);
    }

    [Fact]
    public async Task ResourceOwnership_DriverCannotModifyAnotherDriverTrip()
    {
        var options = CreateInMemoryOptions();
        using (var dbContext = new AlMosaferDbContext(options))
        {
            var driverA = new User { Name = "سائق أ", Email = "a@driver.com", Role = UserRole.Driver };
            var driverB = new User { Name = "سائق ب", Email = "b@driver.com", Role = UserRole.Driver };
            dbContext.Users.AddRange(driverA, driverB);
            await dbContext.SaveChangesAsync();

            var tripOfA = new Trip { DriverId = driverA.Id, FromCity = "تعز", ToCity = "عدن", TripTime = DateTime.UtcNow };
            dbContext.Trips.Add(tripOfA);
            await dbContext.SaveChangesAsync();

            var ownershipService = new ResourceOwnershipService(dbContext);

            var canDriverAEdit = await ownershipService.CanDriverModifyTripAsync(driverA.Id, tripOfA.Id);
            var canDriverBEdit = await ownershipService.CanDriverModifyTripAsync(driverB.Id, tripOfA.Id);

            Assert.True(canDriverAEdit);
            Assert.False(canDriverBEdit);
        }
    }


    [Fact]
    public async Task RequestPasswordReset_UnknownEmail_ReturnsGenericSuccessToPreventEnumeration()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var authService = new AuthService(dbContext, new PasswordHasherService());

        var result = await authService.RequestPasswordResetAsync("ghost@test.com", "http://x/reset?token=__TOKEN__");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task RequestPasswordReset_KnownEmail_StoresSingleUseToken()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        dbContext.Users.Add(new User { Name = "مستخدم", Email = "reset1@test.com", Role = UserRole.Traveler, PasswordHash = "old" });
        await dbContext.SaveChangesAsync();
        var authService = new AuthService(dbContext, new PasswordHasherService());

        await authService.RequestPasswordResetAsync("reset1@test.com", "http://x/reset?token=__TOKEN__");

        var user = await dbContext.Users.FirstAsync(u => u.Email == "reset1@test.com");
        Assert.NotNull(user.PreferencesJson);
        Assert.Contains("PasswordReset", user.PreferencesJson);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_ChangesHash_AndTokenIsSingleUse()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        dbContext.Users.Add(new User { Name = "مستخدم", Email = "reset2@test.com", Role = UserRole.Traveler, PasswordHash = "old" });
        await dbContext.SaveChangesAsync();
        var hasher = new PasswordHasherService();
        var authService = new AuthService(dbContext, hasher);

        await authService.RequestPasswordResetAsync("reset2@test.com", "http://x/reset?token=__TOKEN__");
        var raw = (await dbContext.Users.FirstAsync(u => u.Email == "reset2@test.com")).PreferencesJson!;
        var token = System.Text.Json.JsonDocument.Parse(raw).RootElement.GetProperty("PasswordReset").GetProperty("Token").GetString()!;

        var first = await authService.ResetPasswordAsync(token, "newStrongPass1");
        Assert.True(first.Success);

        var user = await dbContext.Users.FirstAsync(u => u.Email == "reset2@test.com");
        Assert.NotEqual("old", user.PasswordHash);
        Assert.True(hasher.VerifyPassword(user, user.PasswordHash, "newStrongPass1"));

        var second = await authService.ResetPasswordAsync(token, "anotherPass123");
        Assert.False(second.Success);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        dbContext.Users.Add(new User { Name = "مستخدم", Email = "reset3@test.com", Role = UserRole.Traveler, PasswordHash = "old" });
        await dbContext.SaveChangesAsync();
        var authService = new AuthService(dbContext, new PasswordHasherService());

        var result = await authService.ResetPasswordAsync("does-not-exist-token-value-abcdef123456", "newStrongPass1");

        Assert.False(result.Success);
        var user = await dbContext.Users.FirstAsync(u => u.Email == "reset3@test.com");
        Assert.Equal("old", user.PasswordHash);
    }
}
