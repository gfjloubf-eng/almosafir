using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class DbHealthCheckTest
{
    [Fact]
    public async Task CheckConnectionAsync_ReturnsValidHealthStatus()
    {
        var options = new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: "TestHealthDb")
            .Options;

        using var dbContext = new AlMosaferDbContext(options);

        var healthService = new DbConnectionHealthService(dbContext);

        var (isHealthy, status, message) = await healthService.CheckConnectionAsync();

        Assert.NotNull(status);
        Assert.NotEmpty(status);
        Assert.NotNull(message);
    }
}
