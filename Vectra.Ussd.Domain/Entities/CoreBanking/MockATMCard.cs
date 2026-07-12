public class MockATMCard
{
    public int Id { get; set; }
    public int CustomerAccountId { get; set; }
    public CustomerAccount CustomerAccount { get; set; } = null!;
    public cardType Type { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int CVV { get; set; }
    public string CardNumber { get; set; } = null!;
    public cardNetwork Network { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsActivated { get; set; } = false; // distinguish between issued-but-not-activated & activated.
    public string? cardPINHash { get; set; } = default;
    public bool PosEnabled { get; set; } = false;
    public bool AtmEnabled { get; set; } = false;
    public bool WebEnabled { get; set; } = false;
    public bool Contactless { get; set; } = false;
    public bool InternationalUsage { get; set; } = false;
    public enum cardType
    {
        Debit = 1,
        Credit = 2,
        Physical = 3,
        Virtual = 4,
        Prepaid = 5
    }
    public enum cardNetwork
    {
        VISA = 1,
        MasterCard = 2,
        Verve = 3
    }
}