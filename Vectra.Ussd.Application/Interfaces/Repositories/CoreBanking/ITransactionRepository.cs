using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;

public interface ITransactionHistoryRepository
{
    string AddTransaction(string senderAccountNumber, string receiverAccountNumber, decimal amount, TransferType transferType,
         TransactionHistory.TransactionStatus transactionStatus, string? failureReason, string bankName,
         string idempotencyKey, CancellationToken cancellationToken);
    Task<IEnumerable<TransactionHistory>> GetAccountTransactionsAsync(string accountNumber, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<decimal> GetDailyTotalAmountAsync(string accountNumber, TransferType transferType, DateTime date, CancellationToken cancellationToken);
    Task<int> GetDailyTransactionCountAsync(string accountNumber, TransferType transferType, DateTime date, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
