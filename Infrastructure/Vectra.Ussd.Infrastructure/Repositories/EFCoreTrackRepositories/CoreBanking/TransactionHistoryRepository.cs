using Microsoft.EntityFrameworkCore;
using Vectra.Ussd.Application.Interfaces.Repositories.CoreBanking;
using Vectra.Ussd.Domain.Entities.CoreBanking;

namespace Vectra.Ussd.Infrastructure.Repositories;

public class TransactionHistoryRepository : ITransactionHistoryRepository
{
    private readonly MockDbContext _context;

    public TransactionHistoryRepository(MockDbContext context)
    {
        _context = context;
    }

    public string AddTransaction(string senderAccountNumber, string receiverAccountNumber, decimal amount, TransferType transferType,
     TransactionHistory.TransactionStatus transactionStatus, string? failureReason, string bankName,
     string idempotencyKey, CancellationToken cancellationToken)
    {
        TransactionHistory transaction = new TransactionHistory
        {
            TransactionId = Guid.NewGuid().ToString("N"),
            SenderAccountNumber = senderAccountNumber,
            ReceiverAccountNumber = receiverAccountNumber,
            Amount = amount,
            TransferType = transferType,
            Status = transactionStatus,
            FailureReason = transactionStatus != TransactionHistory.TransactionStatus.Success ? failureReason : null,
            BankName = bankName,
            IdempotencyKey = idempotencyKey
        };
        _context.Transactions.AddAsync(transaction, cancellationToken);

        return transaction.TransactionId;
    }
    /*public async Task GetTransactionIdempotencyKey()
    {
        await _context.Transactions.Where();
    }*/

    public async Task<IEnumerable<TransactionHistory>> GetAccountTransactionsAsync(string accountNumber, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(x => x.SenderAccountNumber == accountNumber || x.ReceiverAccountNumber == accountNumber);

        if (startDate.HasValue)
            query = query.Where(x => x.DateCreated >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.DateCreated <= endDate.Value);

        return await query.OrderByDescending(x => x.DateCreated).ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetDailyTotalAmountAsync(string accountNumber, TransferType transferType, DateTime date, CancellationToken cancellationToken)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _context.Transactions
            .AsNoTracking()
            .Where(x => x.SenderAccountNumber == accountNumber &&
                        x.TransferType == transferType &&
                        x.Status == TransactionHistory.TransactionStatus.Success &&
                        x.DateCreated >= dayStart &&
                        x.DateCreated < dayEnd)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    public async Task<int> GetDailyTransactionCountAsync(string accountNumber, TransferType transferType, DateTime date, CancellationToken cancellationToken)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _context.Transactions
            .AsNoTracking()
            .CountAsync(x => x.SenderAccountNumber == accountNumber &&
                             x.TransferType == transferType &&
                             x.Status == TransactionHistory.TransactionStatus.Success &&
                             x.DateCreated >= dayStart &&
                             x.DateCreated < dayEnd, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
