using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Vectra.Ussd.Application.Interfaces.Services;

namespace Vectra.Ussd.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountOpeningOrchestrator, AccountOpeningOrchestrator>();
        services.AddScoped<IMenuOrchestrator, MenuOrchestrator>();
        services.AddScoped<IRegistrationOrchestrator, RegistrationOrchestrator>();
        services.AddScoped<IAccountOpeningService, AccountOpeningService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransferOrchestrator, TransferOrchestrator>();
        services.AddScoped<IAccountBalanceOrchestrator, AccountBalanceOrchestrator>();
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<IServiceAccountNumbersOrchestrator, ServiceAccountNumbersOrchestrator>();
        services.AddScoped<IServiceAccountNumbersService, ServiceAccountNumbersService>();
        services.AddScoped<IPINManagementOrchestrator, PINManagementOrchestrator>();
        services.AddScoped<IPINManagementService, PINManagementService>();
        services.AddScoped<IAirtimeOrchestrator, AirtimeOrchestrator>();
        services.AddScoped<IAirtimeService, AirtimeService>();
        services.AddScoped<IBVNInquiryOrchestrator, BVNInquiryOrchestrator>();
        services.AddScoped<IBVNInquiryService, BVNInquiryService>();
        services.AddScoped<IDataRechargeOrchestrator, DataRechargeOrchestrator>();
        services.AddScoped<IDataRechargeService, DataRechargeService>();
        services.AddScoped<ICardRequestOrchestrator, CardRequestOrchestrator>();
        services.AddScoped<ICardRequestService, CardRequestService>();
        services.AddScoped<ICardManagementOrchestrator, CardManagementOrchestrator>();
        services.AddScoped<ICardManagementService, CardManagementService>();
        services.AddScoped<ICoreAccountValidationService, CoreAccountValidationService>();
        services.AddScoped<IVerfySimChecks, VerfySimChecks>();
        services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
        services.AddSingleton<IHandleSession, HandleSession>();
        services.AddSingleton<IVerifySession, VerifySession>();
        services.AddLogging();

        return services;
    }
}
