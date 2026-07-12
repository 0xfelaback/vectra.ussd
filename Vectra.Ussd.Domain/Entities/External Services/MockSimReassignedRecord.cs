namespace Vectra.Ussd.Mocks;

public class MockSimReassignedRecord
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsReassigned { get; set; } = false;
    public DateTime? LastAssignedDate { get; set; } = null;
    public bool IsServiceAvailable { get; set; } = true;
}
