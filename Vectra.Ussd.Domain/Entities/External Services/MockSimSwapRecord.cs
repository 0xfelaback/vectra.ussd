namespace Vectra.Ussd.Mocks;

public class MockSimSwapRecord
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsSwapped { get; set; } = false;
    public DateTime? LastSwapDate { get; set; } = null;
    public bool IsServiceAvailable { get; set; } = true;
}
