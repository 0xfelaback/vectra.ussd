using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
    }
}