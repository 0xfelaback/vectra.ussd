public interface ICustomerAccountRepository
{
    Task<CustomerAccount> CreateAccountAsync(CustomerAccount account, CancellationToken cancellationToken);
    Task<CustomerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<CustomerAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<CustomerAccount?> CustomerHasAccount(string accountNumber, string phoneNumber, CancellationToken token);
    Task LinkCustomerAccount(int accountId, CancellationToken cancellationToken);
    Task<int> CountAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<int> RemoveCustomerAccount(string accountNumber, CancellationToken token);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
