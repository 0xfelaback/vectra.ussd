using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class CustomerAccountReadRepository : ICustomerAccountReadRepository
{
    private readonly MockDbContext _context;

    public CustomerAccountReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        await _context.CustomerAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

    public async Task<CustomerAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken) =>
        await _context.CustomerAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber, cancellationToken);

}
