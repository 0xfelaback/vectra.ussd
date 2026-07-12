using Vectra.Ussd.Domain.Entities.CoreBanking;

public record CardRequestSession : SessionBase
{
    public List<ServiceAccount>? userAccounts = [];
    public int? accountTier { get; set; } = default;
    public CustomerAccount.AccountType? accountType { get; set; } = null;
    public string? eligibleCardNetworks { get; set; } = null;
    public string? customerAddress { get; set; } = null;
    public string? selectedCardNetwork { get; set; } = null;
    public string? bvn { get; set; } = null;
    public string? customerName { get; set; } = null;
    public int pinTrials { get; set; } = 0;
}