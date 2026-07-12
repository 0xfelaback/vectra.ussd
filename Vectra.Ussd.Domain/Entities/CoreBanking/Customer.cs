public class Customer
{
    public int Id { get; set; }
    public string? PhoneNumber { get; set; } = null;
    public string? BvnNumber { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }
    public string? MiddleName { get; set; }
    public bool IsUssdRegistered { get; set; } = false;
    public string? ussdPin1Hash { get; set; } = null;
    public string? ussdPin2Hash { get; set; } = null;
    public bool IsPinSet { get; set; } = false;
    public int PinTrials { get; set; } = 0;
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
    public virtual ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
    public virtual ICollection<ServiceAccount> ServiceAccounts { get; set; } = new List<ServiceAccount>();

    public enum CustomerStatus
    {
        Active = 1,
        Locked = 2
    }
}