public record AirtimeSession : SessionBase
{
    public decimal? airtimeAmount { get; set; } = null;
    public AirtimeRecharge.AirtimeNetwork? beneficiaryISP { get; set; } = null;
    public AirtimeOperation? airtimeOperation { get; set; } = null;
    public List<ServiceAccount>? userAccounts { get; set; } = [];
    public enum AirtimeOperation
    {
        Self = 1, Others = 2
    }
}