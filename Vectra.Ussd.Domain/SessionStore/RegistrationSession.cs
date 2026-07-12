public record RegistrationSession : RegistrationAndAccountOpeningSessionBase
{
    public IdentityProof proof { get; set; } = IdentityProof.None;
    public string? PIN { get; set; }
    public enum IdentityProof
    {
        None = 0, BVN = 1, ATMCard = 2
    }
}

public record RegistrationAndAccountOpeningSessionBase : SessionBase
{
    public string BVN { get; set; } = default!;
    public bool? SimSwapChecked { get; set; } = false;
    public bool SimReassignedChecked { get; set; } = false;
}