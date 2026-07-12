using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public interface IDataBundleRepository
{
    Task<DataBundle?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<DataBundle>> GetByTelcoAsync(AirtimeRecharge.AirtimeNetwork telco, CancellationToken cancellationToken);
    Task<IEnumerable<DataBundle>> GetActiveBundlesByTelcoAsync(AirtimeRecharge.AirtimeNetwork telco, CancellationToken cancellationToken);
    Task<IEnumerable<DataBundle>> GetAllActiveBundlesAsync(CancellationToken cancellationToken);
}
