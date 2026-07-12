using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public interface IAccountTierRepository
{
    Task<AccountTier?> GetTierLimitAsync(int tierLevel, TransferType transferType, CancellationToken cancellationToken);
    Task<IEnumerable<AccountTier>> GetAllTiersAsync(CancellationToken cancellationToken);
}
