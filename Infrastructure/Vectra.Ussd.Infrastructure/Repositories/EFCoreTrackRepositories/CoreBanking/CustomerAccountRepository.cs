using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class CustomerAccountRepository : ICustomerAccountRepository
{
    private readonly MockDbContext _context;

    public CustomerAccountRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerAccount> CreateAccountAsync(CustomerAccount account, CancellationToken cancellationToken)
    {
        var entry = await _context.CustomerAccounts.AddAsync(account, cancellationToken);
        return entry.Entity;
    }
    public async Task<CustomerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        await _context.CustomerAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    public async Task<CustomerAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
        await _context.CustomerAccounts.FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber, cancellationToken);
    public async Task<CustomerAccount?> CustomerHasAccount(string accountNumber, string phoneNumber, CancellationToken token) => await _context.CustomerAccounts.Where(a => a.AccountNumber == accountNumber && a.PhoneNumber == phoneNumber).AsNoTracking().FirstOrDefaultAsync(token);
    public async Task<int> CountAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
    await _context.CustomerAccounts.Where(a => a.PhoneNumber == phoneNumber).AsNoTracking().CountAsync(cancellationToken);

    /* SetCustomerPrimaryAccount exists on both customer account and service linked accounts */
    public async Task LinkCustomerAccount(int accountId, CancellationToken cancellationToken) => await _context.CustomerAccounts.Where(a => a.Id == accountId).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsLinked, true), cancellationToken);

    public async Task<int> RemoveCustomerAccount(string accountNumber, CancellationToken token)
    {
        return await _context.CustomerAccounts.Where(a => a.AccountNumber == accountNumber).ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, CustomerAccount.AccountStatus.Removed), token);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
