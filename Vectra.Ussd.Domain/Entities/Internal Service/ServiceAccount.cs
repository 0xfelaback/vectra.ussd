public class ServiceAccount
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int CustomerAccountId { get; set; }
    public CustomerAccount CustomerAccount { get; set; } = null!;
    public string PhoneNumber { get; set; } = default!;
    public string BvnNumber { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public bool IsPrimary { get; set; } = false;
    public DateTime? DateLinked { get; set; } = null;
    public DateTime? DateLastModified { get; set; } = null;
}

/* Accounts registered on USSD Channel */