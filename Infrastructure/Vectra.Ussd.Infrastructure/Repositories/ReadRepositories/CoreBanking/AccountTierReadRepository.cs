using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class AccountTierReadRepository : IAccountTierRepository
{
    private readonly MockDbContext _context;

    public AccountTierReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<AccountTier?> GetTierLimitAsync(int tierLevel, TransferType transferType, CancellationToken cancellationToken)
    {
        return await _context.AccountTiers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TierLevel == tierLevel && x.TransferType == transferType && x.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<AccountTier>> GetAllTiersAsync(CancellationToken cancellationToken)
    {
        return await _context.AccountTiers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
