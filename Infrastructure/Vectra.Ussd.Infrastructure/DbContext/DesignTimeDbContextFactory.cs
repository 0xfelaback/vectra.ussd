using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Vectra.Ussd.Infrastructure;

public class VectraUssdContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
{
    public ProjectDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("/Users/great/Desktop/cs/Vectra.Ussd/Vectra.Ussd.Api/appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("localConnectionString"));
        return new ProjectDbContext(optionsBuilder.Options);
    }
}