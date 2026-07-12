public record BVNInquirySession : SessionBase
{
    public int pinTrials { get; set; } = 0;
    public string? enteredPin { get; set; } = null;
    public bool isPinValidated { get; set; } = false;
}