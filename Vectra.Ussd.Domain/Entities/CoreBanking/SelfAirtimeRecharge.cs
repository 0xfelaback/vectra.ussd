public class AirtimeRecharge
{
    public int Id { get; set; }
    public string SenderPhoneNumber { get; set; } = null!;
    public string BeneficiaryPhoneNumber { get; set; } = null!;
    public string RemitterAccountNumber { get; set; } = null!;
    public decimal Amount { get; set; }
    public AirtimeNetwork Network { get; set; }
    public TransactionStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public string? AggregatorReference { get; set; }
    public bool IsSelfRecharge { get; set; } = false;
    public string Channel { get; set; } = "USSD";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public enum AirtimeNetwork
    {
        MTN = 1, Airtel = 2, Globacom = 3, NineMobile = 4
    }
    public enum TransactionStatus
    {
        Success = 1,
        Failed = 2,
        Pending = 3
    }
}

