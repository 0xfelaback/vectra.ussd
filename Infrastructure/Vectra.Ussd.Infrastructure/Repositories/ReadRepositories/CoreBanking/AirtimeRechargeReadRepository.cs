using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class AirtimeRechargeReadRepository : IAirtimeRechargeReadRepository
{
    private readonly MockDbContext _context;

    public AirtimeRechargeReadRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<AirtimeRecharge?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<AirtimeRecharge?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().FirstOrDefaultAsync(x => x.TransactionReference == transactionReference, cancellationToken);

    public async Task<IEnumerable<AirtimeRecharge>> GetBySenderPhoneNumberAsync(string senderPhoneNumber, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().Where(x => x.SenderPhoneNumber == senderPhoneNumber).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IEnumerable<AirtimeRecharge>> GetByBeneficiaryPhoneNumberAsync(string beneficiaryPhoneNumber, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().Where(x => x.BeneficiaryPhoneNumber == beneficiaryPhoneNumber).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IEnumerable<AirtimeRecharge>> GetByRemitterAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().Where(x => x.RemitterAccountNumber == accountNumber).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IEnumerable<AirtimeRecharge>> GetByStatusAsync(AirtimeRecharge.TransactionStatus status, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.AsNoTracking().Where(x => x.Status == status).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
