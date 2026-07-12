using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vectra.Ussd.Infrastructure;
public static class DbConfig
{
    public static IServiceCollection UseSqlite(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ProjectDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

}