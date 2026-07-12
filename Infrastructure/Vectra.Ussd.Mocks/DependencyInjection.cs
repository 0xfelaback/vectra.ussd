using Microsoft.Extensions.DependencyInjection;
using Vectra.Ussd.Application.Interfaces;

namespace Vectra.Ussd.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMocks(this IServiceCollection services)
    {
        services.AddDbContext<MockDbContext>();
        services.AddScoped<INIBSSIdentityVerificationService, NIBSSIdentityVerificationService>();
        services.AddScoped<ICamuService, CamuService>();
        return services;
    }
}