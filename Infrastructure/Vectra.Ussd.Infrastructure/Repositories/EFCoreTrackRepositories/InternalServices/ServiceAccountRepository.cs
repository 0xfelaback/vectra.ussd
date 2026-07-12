using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class ServiceAccountRepository : IServiceAccountRepository
{
    private readonly MockDbContext _context;
    public ServiceAccountRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceAccount> CreateAccountAsync(ServiceAccount account, CancellationToken cancellationToken)
    {
        var entry = await _context.ServiceAccounts.AddAsync(account, cancellationToken);
        return entry.Entity;
    }
    public async Task<ServiceAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        await _context.ServiceAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    public async Task<ServiceAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
        await _context.ServiceAccounts.FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber, cancellationToken);

    public async Task CreateServiceAccountAsync(ServiceAccount account, CancellationToken cancellationToken)
    {
        await _context.ServiceAccounts.AddAsync(account, cancellationToken);
    }
    public async Task<IEnumerable<ServiceAccount?>> GetAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
    await _context.ServiceAccounts.Where(a => a.PhoneNumber == phoneNumber).AsNoTracking().ToListAsync(cancellationToken);
    public async Task<IEnumerable<ServiceAccount>> GetAllByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
        await _context.ServiceAccounts
            //.Include(a => a.Customer)
            //.Include(a => a.CustomerAccount)
            .Where(a => a.PhoneNumber == phoneNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    public async Task<ServiceAccount?> GetFirstAccountByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
    await _context.ServiceAccounts.Where(a => a.PhoneNumber == phoneNumber).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    public async Task<int> CountAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
    await _context.ServiceAccounts.Where(a => a.PhoneNumber == phoneNumber).AsNoTracking().CountAsync(cancellationToken);

    public void DeleteAccountAsync(ServiceAccount account) =>
        _context.ServiceAccounts.Remove(account);

    public async Task<int> MakeAccountDormant(string accountNumber, CancellationToken cancellationToken)
    {
        return await _context.ServiceAccounts.Where(a => a.CustomerAccount.AccountNumber == accountNumber).ExecuteUpdateAsync(s => s.SetProperty(b => b.CustomerAccount.Status, CustomerAccount.AccountStatus.Dormant), cancellationToken);
    }
    public async Task<ServiceAccount?> GetCustomerPrimaryAccount(string phoneNumber, CancellationToken token) => await _context.ServiceAccounts.Where(a => a.PhoneNumber == phoneNumber && a.IsPrimary == true).AsNoTracking().FirstOrDefaultAsync(token);

    public async Task<int> SetCustomerPrimaryAccount(string accountNumber, CancellationToken token)
    {
        await _context.ServiceAccounts.Where(a => a.IsPrimary == true).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsPrimary, false), token);
        return await _context.ServiceAccounts.Where(a => a.AccountNumber == accountNumber).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsPrimary, true), token);
    }
    public async Task<int> RemoveCustomerPrimaryAccount(string accountNumber, CancellationToken token)
    {
        return await _context.ServiceAccounts.Where(a => a.AccountNumber == accountNumber && a.IsPrimary == true).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsPrimary, false), token);
    }

    public async Task<ServiceAccount?> CustomerHasAccount(string accountNumber, string phoneNumber, CancellationToken token) => await _context.ServiceAccounts.Where(a => a.AccountNumber == accountNumber && a.PhoneNumber == phoneNumber).AsNoTracking().FirstOrDefaultAsync(token);

    public async Task<MockATMCard?> GetSingleActiveATMCard(string accountNumber, CancellationToken cancellationToken) =>
        await _context.ServiceAccounts
        .AsNoTracking()
        .Where(a => a.AccountNumber == accountNumber)
        .SelectMany(a => a.CustomerAccount.Cards)
        .Where(a => a.IsActive == true)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<MockATMCard>?> GetAllActiveATMCard(string accountNumber, CancellationToken cancellationToken) =>
        await _context.ServiceAccounts.AsNoTracking().Where(a => a.AccountNumber == accountNumber).SelectMany(a => a.CustomerAccount.Cards).Where(a => a.IsActive == true).ToListAsync();
    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}

