using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Mocks;
using Vectra.Ussd.Domain.Entities.CoreBanking;

public class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
{
    public DbSet<MockBvnRecord> MockBvnRecords { get; set; }
    public DbSet<MockSimSwapRecord> MockNibssSimSwapRecords { get; set; }
    public DbSet<MockSimReassignedRecord> MockNibssSimReassignedRecords { get; set; }
    public DbSet<MockFraudFlag> MockFraudFlags { get; set; }
    public DbSet<ServiceAccount> ServiceAccounts { get; set; }
    public DbSet<CustomerAccount> CustomerAccounts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<MockATMCard> MockATMCards { get; set; }
    public DbSet<NubanInterbankAccounts> Nubans { get; set; }
    public DbSet<AccountTier> AccountTiers { get; set; }
    public DbSet<DataBundle> DataBundles { get; set; }
    public DbSet<TransactionHistory> Transactions { get; set; }
    public DbSet<AirtimeRecharge> AirtimeRecharges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MockDbContext).Assembly);
    }
}