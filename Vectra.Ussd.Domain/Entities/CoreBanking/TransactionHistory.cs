namespace Vectra.Ussd.Domain.Entities.CoreBanking;

public class TransactionHistory
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = default!;
    public string SenderAccountNumber { get; set; } = default!;
    public string ReceiverAccountNumber { get; set; } = default!;
    public decimal Amount { get; set; }
    public TransferType TransferType { get; set; }
    public TransactionStatus Status { get; set; }
    public string? FailureReason { get; set; } = null;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? BankName { get; set; } = null;
    public string? IdempotencyKey { get; set; } = null;
    public enum TransactionStatus
    {
        Success = 1,
        Failed = 2,
        Pending = 3
    }
}


