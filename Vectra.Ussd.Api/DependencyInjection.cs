namespace Vectra.Ussd.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, string mockConnectionstring, string realConnectionString, string simulatorLocalDomain)
    {
        services.AddControllers().AddXmlSerializerFormatters();
        services.AddCors(options => options.AddPolicy("AllowLocalFrontend", policy =>
        {
            policy.WithOrigins(simulatorLocalDomain).AllowAnyHeader().AllowAnyMethod();
        }));
        DbConfig.UseSqlite(services, mockConnectionstring);
        MockDbConfig.UseSqlite(services, realConnectionString);
        services.AddProblemDetails();
        services.AddExceptionHandler<ArgumentOutOfRangeExceptionHandler>();
        services.AddExceptionHandler<InputValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddAutoMapper(config => { }, AppDomain.CurrentDomain.GetAssemblies());
        return services;
    }
}
