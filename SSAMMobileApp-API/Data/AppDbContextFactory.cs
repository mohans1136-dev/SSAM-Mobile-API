using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SSAMMobileApp.Data;

/// <summary>
/// Design-time factory. The EF Core CLI tools (dotnet ef ...) use this to build
/// an AppDbContext WITHOUT starting the whole web app. It reads the same
/// configuration files, so `dotnet ef` and the running app stay in sync.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<AppDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("SqlDb"))
            .Options;

        return new AppDbContext(options);
    }
}
