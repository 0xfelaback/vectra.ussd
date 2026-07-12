using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class AirtimeRechargeRepository : IAirtimeRechargeRepository
{
    private readonly MockDbContext _context;

    public AirtimeRechargeRepository(MockDbContext context)
    {
        _context = context;
    }

    public async Task<string?> CreateAirtimeRechargeAsync(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken)
    {
        AirtimeRecharge recharge = new AirtimeRecharge
        {
            SenderPhoneNumber = senderPhoneNumber,
            BeneficiaryPhoneNumber = beneficiaryPhoneNumber,
            RemitterAccountNumber = remitterAccountNumber,
            Amount = amount,
            Network = network,
            Status = status,
            IsSelfRecharge = isSelfRecharge,
            TransactionReference = Guid.NewGuid().ToString("N"),
            /* TODO: REQUIRES TO BE TREATED AS EXTERNAL SERVICE */
            AggregatorReference = Guid.NewGuid().ToString("N")
        };
        await _context.AirtimeRecharges.AddAsync(recharge, cancellationToken);
        return recharge.TransactionReference;
    }

    public async Task<AirtimeRecharge?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<AirtimeRecharge?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges.FirstOrDefaultAsync(x => x.TransactionReference == transactionReference, cancellationToken);

    public async Task<int> UpdateStatusAsync(int id, AirtimeRecharge.TransactionStatus status, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status), cancellationToken);

    public async Task<int> UpdateTransactionReferenceAsync(int id, string transactionReference, CancellationToken cancellationToken) =>
        await _context.AirtimeRecharges
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.TransactionReference, transactionReference), cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _context.SaveChangesAsync(cancellationToken);
}
