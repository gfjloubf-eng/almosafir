using AlMosafer.Application.Interfaces;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class DbConnectionHealthService : IDbConnectionHealthService
{
    private readonly AlMosaferDbContext _dbContext;

    public DbConnectionHealthService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool CanConnect, string Message, string DatabaseName)> CheckConnectionAsync()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            var dbName = _dbContext.Database.GetDbConnection().Database;
            
            if (canConnect)
            {
                return (true, "تم الاتصال بقاعدة بيانات MySQL بنجاح عبر Entity Framework Core!", dbName);
            }
            else
            {
                return (false, "تعذر الاتصال بقاعدة البيانات. يُرجى التاكد من تشغيل خادم MySQL في XAMPP.", dbName);
            }
        }
        catch (Exception ex)
        {
            return (false, $"حدث خطأ أثناء الاتصال بقاعدة البيانات: {ex.Message}", "mosafir_db");
        }
    }
}
