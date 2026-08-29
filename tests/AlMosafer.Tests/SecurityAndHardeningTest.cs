using AlMosafer.Application.DTOs.Reports;
using AlMosafer.Domain.Entities;
using AlMosafer.Infrastructure.Services;
using AlMosafer.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace AlMosafer.Tests;

public class SecurityAndHardeningTest
{
    [Fact]
    public void AdminController_ProtectedWithAdminRoleAuthorizeAttribute()
    {
        var controllerType = typeof(AdminController);
        var authorizeAttributes = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        Assert.NotEmpty(authorizeAttributes);
        var authAttr = (AuthorizeAttribute)authorizeAttributes[0];
        Assert.Equal("Admin", authAttr.Roles);
    }

    [Fact]
    public void TravelerController_ProtectedWithTravelerRoleAuthorizeAttribute()
    {
        var controllerType = typeof(TravelerController);
        var authorizeAttributes = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        Assert.NotEmpty(authorizeAttributes);
        var authAttr = (AuthorizeAttribute)authorizeAttributes[0];
        Assert.Contains("Traveler", authAttr.Roles);
    }

    [Fact]
    public void DriverController_ProtectedWithDriverRoleAuthorizeAttribute()
    {
        var controllerType = typeof(DriverController);
        var authorizeAttributes = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        Assert.NotEmpty(authorizeAttributes);
        var authAttr = (AuthorizeAttribute)authorizeAttributes[0];
        Assert.Contains("Driver", authAttr.Roles);
    }

    [Fact]
    public void PasswordHasherService_GeneratesSaltedHash_AndVerifiesCorrectly()
    {
        var hasher = new PasswordHasherService();
        var dummyUser = new User { Email = "test@almosafir.com" };
        var rawPassword = "SecurePassword@2026";

        var hash = hasher.HashPassword(dummyUser, rawPassword);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(rawPassword, hash);
        Assert.True(hasher.VerifyPassword(dummyUser, hash, rawPassword));
        Assert.False(hasher.VerifyPassword(dummyUser, hash, "WrongPassword"));
    }

    [Fact]
    public void ReportFilterDto_AllowsValidDateRange()
    {
        var filter = new ReportFilterDto
        {
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 12, 31)
        };

        Assert.True(filter.FromDate <= filter.ToDate);
    }

    [Fact]
    public void OperationResult_Success_MapsStatusCodeAndPayloadCorrectly()
    {
        var result = AlMosafer.Application.Common.OperationResult<string>.Success("OK", 200);

        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Data);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void OperationResult_Failure_MapsErrorAndStatusCodeCorrectly()
    {
        var result = AlMosafer.Application.Common.OperationResult<string>.NotFound("غير موجود");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("غير موجود", result.ErrorMessage);
    }
}
