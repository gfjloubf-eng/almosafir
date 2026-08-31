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
}
