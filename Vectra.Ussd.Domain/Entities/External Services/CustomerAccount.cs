public class CustomerAccount
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string PhoneNumber { get; set; } = default!;
    public string BvnNumber { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public AccountType Type { get; set; } = AccountType.Savings;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public int TierLevel { get; set; } = 1;
    public decimal Balance { get; set; } = 0m;
    public bool HasPnd { get; set; } = false;
    public bool IsUssdRegistered { get; set; } = false;
    public bool IsLinked { get; set; } = false;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateLastModified { get; set; } = null;

    public string ImageUrl { get; set; } = null!;
    public string? SignatureUrl { get; set; } = null;

    public virtual ICollection<MockATMCard> Cards { get; set; } = new List<MockATMCard>();

    public ServiceAccount ServiceAccount { get; set; } = null!;

    public enum AccountType
    {
        Savings = 1,
        Current = 2,
        FixedDeposit = 3,
        Domiciliary = 4
    }

    public enum AccountStatus
    {
        Active = 1,
        Removed = 2,
        Pnd = 3,
        Dormant = 4
    }
}
