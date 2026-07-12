public class MockFraudFlag
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string BvnNumber { get; set; } = null!;
    public FlagReason Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public FraudStatus Status { get; set; }
    // populate if an account was successfully created
    public string? AccountNumber { get; set; } = null;

    public MockFraudFlag()
    {
        Timestamp = DateTime.UtcNow;
        Status = FraudStatus.PendingReview;
    }
}

public enum FraudStatus
{
    PendingReview,
    Cleared,
    ConfirmedFraud
}
public enum FlagReason
{
    SwappedSIM,
    ReassignedSIM,
    SwappedSimandReassignedSim,
    ServiceDown
}
