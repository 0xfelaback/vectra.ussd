using Microsoft.Extensions.DependencyInjection;
using Vectra.Ussd.Application.Interfaces;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Infrastructure.Repositories;

namespace Vectra.Ussd.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string redisConnectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        services.AddScoped<IBvnReadRepository, BvnReadRepository>();
        services.AddScoped<IISimActionsRepository, SimSwapReadRepository>();
        services.AddScoped<IFraudFlagRepository, FraudFlagRepository>();
        services.AddScoped<ICustomerAccountRepository, CustomerAccountRepository>();
        services.AddScoped<ICustomerAccountReadRepository, CustomerAccountReadRepository>();
        services.AddScoped<IServiceAccountRepository, ServiceAccountRepository>();
        services.AddScoped<IMockATMCardRepository, MockATMCardRepository>();
        services.AddScoped<INubanRepository, NubanRepository>();
        services.AddScoped<IAccountTierRepository, AccountTierReadRepository>();
        services.AddScoped<IDataBundleRepository, DataBundleReadRepository>();
        services.AddScoped<IDataPurchaseRepository, DataPurchaseRepository>();
        services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();
        services.AddScoped<IAirtimeRechargeRepository, AirtimeRechargeRepository>();
        services.AddScoped<IAirtimeRechargeReadRepository, AirtimeRechargeReadRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddDbContext<ProjectDbContext>();
        return services;
    }
}

