namespace Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public interface IAirtimeRechargeRepository
{
    Task<string?> CreateAirtimeRechargeAsync(string senderPhoneNumber, string beneficiaryPhoneNumber, string remitterAccountNumber,
     decimal amount, AirtimeRecharge.AirtimeNetwork network, AirtimeRecharge.TransactionStatus status,
      bool isSelfRecharge, CancellationToken cancellationToken);
    Task<AirtimeRecharge?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AirtimeRecharge?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken);
    Task<int> UpdateStatusAsync(int id, AirtimeRecharge.TransactionStatus status, CancellationToken cancellationToken);
    Task<int> UpdateTransactionReferenceAsync(int id, string transactionReference, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
