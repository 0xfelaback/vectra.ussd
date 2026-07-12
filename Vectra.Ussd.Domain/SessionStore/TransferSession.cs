public record TransferSession : SessionBase
{
    public string beneficiaryAccountNumber { get; set; } = default!;
    public string? beneficiaryFirstName { get; set; } = default;
    public string? beneficiaryLastName { get; set; } = default;
    public string? beneficiaryBankCode { get; set; } = default;
    public string beneficiaryBankName { get; set; } = default!;
    public decimal transferAmount { get; set; } = 0m;
    public decimal remitterAccountbalance { get; set; } = 0m;
    public int accountTier { get; set; } = default;
    public string? ussdPin { get; set; } = default!;
    public int pinTrials { get; set; } = default;
    public bool fromMenu { get; set; } = false;
    public bool isIntraBankTransfer { get; set; } = false;
    public List<ServiceAccount>? userAccounts = [];
    public string idempotencykey { get; set; } = default!;
}