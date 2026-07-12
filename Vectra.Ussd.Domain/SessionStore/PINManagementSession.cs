using System.Globalization;

public record PINManagementSession : SessionBase
{
    public PINOperations? pinOperation { get; set; } = null;
    public PINAction? pinActions { get; set; } = null;
    public IdentityProof proof { get; set; } = IdentityProof.None;
    public string? ussdPin1Hash { get; set; } = null;
    public string? ussdPin2Hash { get; set; } = null;
    public DateOnly? DOB { get; set; } = null;

    public enum PINOperations
    {
        CreatePIN = 1, ResetPIN = 2
    }
    public enum PINAction
    {
        createPIN1 = 1, createPIN2 = 2, resetPIN1 = 3, resetPIN2 = 4
    }
    public enum IdentityProof
    {
        None = 0, BVN = 1, ATMCard = 2
    }
}