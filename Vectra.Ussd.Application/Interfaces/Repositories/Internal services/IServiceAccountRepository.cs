public interface IServiceAccountRepository
{
    Task<ServiceAccount> CreateAccountAsync(ServiceAccount account, CancellationToken cancellationToken);
    Task<ServiceAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<ServiceAccount?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<ServiceAccount?> GetFirstAccountByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task CreateServiceAccountAsync(ServiceAccount account, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceAccount?>> GetAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceAccount>> GetAllByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    void DeleteAccountAsync(ServiceAccount account);
    Task<int> MakeAccountDormant(string accountNumber, CancellationToken cancellationToken);
    Task<ServiceAccount?> GetCustomerPrimaryAccount(string phoneNumber, CancellationToken token);
    Task<int> SetCustomerPrimaryAccount(string accountNumber, CancellationToken token);
    Task<int> RemoveCustomerPrimaryAccount(string accountNumber, CancellationToken token);
    Task<ServiceAccount?> CustomerHasAccount(string accountNumber, string phoneNumber, CancellationToken token);
    Task<MockATMCard?> GetSingleActiveATMCard(string accountNumber, CancellationToken cancellationToken);
    Task<int> CountAccountsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<List<MockATMCard>?> GetAllActiveATMCard(string accountNumber, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}