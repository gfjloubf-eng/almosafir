using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlMosafer.Infrastructure.Persistence;

public class AlMosaferDbContextFactory : IDesignTimeDbContextFactory<AlMosaferDbContext>
{
    public AlMosaferDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AlMosaferDbContext>();
        
        var connectionString = "Server=localhost;Port=3306;Database=mosafir_db;User=root;Password=;";
        var serverVersion = new MySqlServerVersion(new Version(10, 4, 32));

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AlMosaferDbContext(optionsBuilder.Options);
    }
}
