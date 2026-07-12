using Microsoft.EntityFrameworkCore;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class NubanRepository : INubanRepository
{
    private readonly MockDbContext _context;

    public NubanRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<NubanInterbankAccounts?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        await _context.Nubans.FirstOrDefaultAsync(n => n.AccountNumber == accountNumber, cancellationToken);

    public async Task<IEnumerable<NubanInterbankAccounts>> GetByBankCodeAsync(string bankCode, CancellationToken cancellationToken) =>
        await _context.Nubans.Where(n => n.BankCode == bankCode).ToListAsync(cancellationToken);

    public async Task CreateNubanAsync(NubanInterbankAccounts nuban, CancellationToken cancellationToken) =>
        await _context.Nubans.AddAsync(nuban, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}