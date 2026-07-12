public record AccountManagementSession : SessionBase
{
    public int customerAccountId { get; set; }
    public AccountManagementOperation? accountManagementOperation = null;
    public enum AccountManagementOperation
    {
        SetPrimaryAccount = 1, AddAccount = 2, RemoveAccount = 3
    }
}