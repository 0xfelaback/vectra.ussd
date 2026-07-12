namespace Vectra.Ussd.Domain.Entities.CoreBanking;

public class AccountTier
{
    public int Id { get; set; }
    public int TierLevel { get; set; }
    public TransferType TransferType { get; set; }
    public decimal SingleTransactionLimit { get; set; }
    public decimal DailyTransactionLimit { get; set; }
    public int DailyTransactionCount { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum TransferType
{
    Intrabank = 1,
    Interbank = 2
}
