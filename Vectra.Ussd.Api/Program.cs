using Vectra.Ussd.Api;
using Vectra.Ussd.Application;
using Vectra.Ussd.Infrastructure;
using Vectra.Ussd.Domain;
using Vectra.Ussd.Mocks;
using System.Data;
using Vectra.Ussd.Domain.Entities.CoreBanking;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDomain().AddApplication().AddMocks();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("redisConnectionString") ?? string.Empty)
.AddWebApi(builder.Configuration.GetConnectionString("localConnectionString")
 ?? string.Empty, builder.Configuration.GetConnectionString("mockConnectionstring") ?? string.Empty,
 builder.Configuration.GetConnectionString("simulatorLocalDomain") ?? string.Empty);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();

app.UseAuthorization();
app.UseCors("AllowLocalFrontend");
app.MapStaticAssets();

app.MapControllers();

using (var scope = app.Services.CreateAsyncScope())
{
    MockDbContext dbContext = scope.ServiceProvider.GetRequiredService<MockDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!dbContext.MockBvnRecords.Any())
    {
        var records = DateSeeder.GenerateMockRecords();

        await dbContext.MockBvnRecords.AddRangeAsync(records["bvn"].Cast<MockBvnRecord>().ToList());
        await dbContext.MockNibssSimSwapRecords.AddRangeAsync(records["simswap"].Cast<MockSimSwapRecord>().ToList());
        await dbContext.MockNibssSimReassignedRecords.AddRangeAsync(records["reassigned"].Cast<MockSimReassignedRecord>().ToList());
        await dbContext.Customers.AddRangeAsync(records["customers"].Cast<Customer>().ToList());
        await dbContext.CustomerAccounts.AddRangeAsync(records["accounts"].Cast<CustomerAccount>().ToList());
        await dbContext.ServiceAccounts.AddRangeAsync(records["serviceAccounts"].Cast<ServiceAccount>().ToList());
        await dbContext.Nubans.AddRangeAsync(records["nuban"].Cast<NubanInterbankAccounts>().ToList());
        await dbContext.AccountTiers.AddRangeAsync(records["accountTiers"].Cast<AccountTier>().ToList());
        await dbContext.DataBundles.AddRangeAsync(records["dataBundles"].Cast<DataBundle>().ToList());
        await dbContext.Transactions.AddRangeAsync(records["transactions"].Cast<TransactionHistory>().ToList());
        await dbContext.AirtimeRecharges.AddRangeAsync(records["airtimeRecharges"].Cast<AirtimeRecharge>().ToList());
        await dbContext.SaveChangesAsync();
    }
}


app.Run();
