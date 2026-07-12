using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class MockDbConfig
{
    public static IServiceCollection UseSqlite(this IServiceCollection services, string mockConnectionstring)
    {
        services.AddDbContext<MockDbContext>(options => options.UseSqlite(mockConnectionstring));
        return services;
    }
}