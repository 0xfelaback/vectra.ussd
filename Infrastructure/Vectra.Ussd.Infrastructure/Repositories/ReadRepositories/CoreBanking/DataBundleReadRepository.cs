using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class DataBundleReadRepository : IDataBundleRepository
{
    private readonly MockDbContext _context;

    public DataBundleReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<DataBundle?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _context.DataBundles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IEnumerable<DataBundle>> GetByTelcoAsync(AirtimeRecharge.AirtimeNetwork telco, CancellationToken cancellationToken) =>
        await _context.DataBundles.AsNoTracking().Where(x => x.Telco == telco).OrderBy(x => x.BundleName).ToListAsync(cancellationToken);

    public async Task<IEnumerable<DataBundle>> GetActiveBundlesByTelcoAsync(AirtimeRecharge.AirtimeNetwork telco, CancellationToken cancellationToken) =>
        await _context.DataBundles.AsNoTracking().Where(x => x.Telco == telco && x.IsActive).OrderBy(x => x.DataSizeMB).ToListAsync(cancellationToken);

    public async Task<IEnumerable<DataBundle>> GetAllActiveBundlesAsync(CancellationToken cancellationToken) =>
        await _context.DataBundles.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Telco).ThenBy(x => x.DataSizeMB).ToListAsync(cancellationToken);
}
