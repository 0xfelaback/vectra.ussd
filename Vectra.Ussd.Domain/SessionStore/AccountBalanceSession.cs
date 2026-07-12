public record AccountBalanceSession : SessionBase
{
    public List<ServiceAccount>? userAccounts = [];
}