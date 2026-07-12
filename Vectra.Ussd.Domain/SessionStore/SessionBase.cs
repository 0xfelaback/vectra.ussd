public record SessionBase
{
    public string SessionId { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string accountNumber { get; set; } = default!;
    public int CurrentStep { get; set; } = 0; // 1-indexed-based counting, from specific option page not menu
    public SessionSub? sub { get; set; }
    public int remitterTierLevel { get; set; }
    public DateTime? CreatedAt { get; set; } = null;
}

public enum SessionSub
{
    accountOpening = 1, registration = 2, mainMenu = 3, Transfer = 4, Airtime = 5, accountBalance = 6,
    checkAccountNumber = 8, DataPurchase = 10, BvnCheck = 11, PINManagement = 13, CardRequest = 14, CardManagement = 15, accountManagement = 18
}