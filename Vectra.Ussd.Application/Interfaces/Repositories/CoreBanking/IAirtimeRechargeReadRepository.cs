namespace Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public interface IAirtimeRechargeReadRepository
{
    Task<AirtimeRecharge?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AirtimeRecharge?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken cancellationToken);
    Task<IEnumerable<AirtimeRecharge>> GetBySenderPhoneNumberAsync(string senderPhoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<AirtimeRecharge>> GetByBeneficiaryPhoneNumberAsync(string beneficiaryPhoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<AirtimeRecharge>> GetByRemitterAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<IEnumerable<AirtimeRecharge>> GetByStatusAsync(AirtimeRecharge.TransactionStatus status, CancellationToken cancellationToken);
}
