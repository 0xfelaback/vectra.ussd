using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Vectra.Ussd.Infrastructure;

public class MockContextFactory : IDesignTimeDbContextFactory<MockDbContext>
{
    public MockDbContext CreateDbContext(string[] args)
    {
        var projectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Vectra.Ussd.Api"));
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<MockDbContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("mockConnectionstring"));
        return new MockDbContext(optionsBuilder.Options);
    }
}